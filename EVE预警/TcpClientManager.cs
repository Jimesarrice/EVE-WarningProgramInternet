using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EVE预警
{
    /// <summary>
    /// TCP 客户端管理器 - 管理与服务器的连接、消息收发、心跳和重连
    /// </summary>
    public class TcpClientManager : IDisposable
    {
        // 连接相关
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private bool _isConnected = false;
        private string _serverIp = "";
        private int _serverPort = 2424;
        private string _username = "";
        private string _galaxy = "";
        private CancellationTokenSource? _receiveCts;
        private System.Threading.Timer? _heartbeatTimer;

        // 预警去重
        private Dictionary<ImageProcessor.AlertType, int> _lastSentAlerts = new();
        private readonly object _alertsLock = new object();

        // 重连相关
        private int _reconnectAttempts = 0;
        private const int MaxReconnectAttempts = 5;
        private readonly int[] _reconnectDelays = { 3, 6, 12, 24, 30 };

        // 事件
        public event EventHandler<string>? MessageReceived;
        public event EventHandler<bool>? ConnectionStateChanged;
        public event EventHandler<string>? ErrorOccurred;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public async Task<bool> ConnectAsync(string serverIp, int port, string username, string galaxy)
        {
            try
            {
                _serverIp = serverIp;
                _serverPort = port;
                _username = username;
                _galaxy = galaxy;

                _tcpClient = new TcpClient();
                _receiveCts = new CancellationTokenSource();

                // 连接服务器（5秒超时）
                using var cts = new CancellationTokenSource(5000);
                await _tcpClient.ConnectAsync(serverIp, port, cts.Token);

                _networkStream = _tcpClient.GetStream();
                _isConnected = true;
                _reconnectAttempts = 0;

                // 发送注册消息
                await SendRegistrationAsync();

                // 启动心跳定时器
                _heartbeatTimer = new System.Threading.Timer(
                    HeartbeatCallback, null, 30000, 30000);

                // 启动接收循环
                _ = ReceiveLoopAsync();

                ConnectionStateChanged?.Invoke(this, true);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"连接失败: {ex.Message}");
                _isConnected = false;
                Cleanup();
                return false;
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            _reconnectAttempts = MaxReconnectAttempts; // 阻止自动重连

            if (_isConnected)
            {
                try
                {
                    // 发送断开通知（可选）
                    if (_networkStream?.CanWrite == true)
                    {
                        var message = new { type = "disconnect" };
                        string json = JsonSerializer.Serialize(message) + "\n";
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await _networkStream.WriteAsync(bytes);
                    }
                }
                catch { }
            }

            Cleanup();
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);

            // 清空上次发送的预警
            lock (_alertsLock)
            {
                _lastSentAlerts.Clear();
            }
        }

        /// <summary>
        /// 发送预警数据（含去重）
        /// </summary>
        public async Task<bool> SendAlertAsync(Dictionary<ImageProcessor.AlertType, int> alerts, string galaxy)
        {
            if (!_isConnected || _networkStream == null) return false;

            // 检查是否与上次相同
            lock (_alertsLock)
            {
                if (AreAlertsEqual(alerts, _lastSentAlerts))
                {
                    return false; // 相同，不重复发送
                }
            }

            try
            {
                // 构建预警消息
                var alertStrings = alerts
                    .Where(kvp => kvp.Value > 0)
                    .Select(kvp => $"{kvp.Key}:{kvp.Value}")
                    .ToArray();

                if (alertStrings.Length == 0) return false;

                var messageObj = new
                {
                    type = "alert",
                    galaxy = galaxy,
                    alerts = alertStrings
                };

                string json = JsonSerializer.Serialize(messageObj) + "\n";
                var bytes = Encoding.UTF8.GetBytes(json);

                await _networkStream.WriteAsync(bytes);
                await _networkStream.FlushAsync();

                // 更新上次发送的预警
                lock (_alertsLock)
                {
                    _lastSentAlerts = new Dictionary<ImageProcessor.AlertType, int>(alerts);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"发送预警失败: {ex.Message}");
                HandleDisconnect();
                return false;
            }
        }

        /// <summary>
        /// 发送心跳
        /// </summary>
        private async Task SendHeartbeatAsync()
        {
            if (!_isConnected || _networkStream == null) return;

            try
            {
                var message = new { type = "heartbeat" };
                string json = JsonSerializer.Serialize(message) + "\n";
                var bytes = Encoding.UTF8.GetBytes(json);

                await _networkStream.WriteAsync(bytes);
                await _networkStream.FlushAsync();
            }
            catch
            {
                HandleDisconnect();
            }
        }

        /// <summary>
        /// 发送注册消息
        /// </summary>
        private async Task SendRegistrationAsync()
        {
            if (_networkStream == null) return;

            var messageObj = new
            {
                type = "register",
                username = _username,
                galaxy = _galaxy
            };

            string json = JsonSerializer.Serialize(messageObj) + "\n";
            var bytes = Encoding.UTF8.GetBytes(json);

            await _networkStream.WriteAsync(bytes);
            await _networkStream.FlushAsync();
        }

        /// <summary>
        /// 接收消息循环
        /// </summary>
        private async Task ReceiveLoopAsync()
        {
            if (_networkStream == null || _receiveCts == null) return;

            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();

            try
            {
                while (!_receiveCts.Token.IsCancellationRequested && _isConnected)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, _receiveCts.Token);
                    if (bytesRead == 0)
                    {
                        // 服务器断开
                        HandleDisconnect();
                        return;
                    }

                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    messageBuilder.Append(received);

                    // 处理完整消息
                    while (messageBuilder.Length > 0)
                    {
                        string msg = messageBuilder.ToString();
                        int newlineIndex = msg.IndexOf('\n');
                        if (newlineIndex == -1) break;

                        string message = msg.Substring(0, newlineIndex).Trim();
                        messageBuilder.Remove(0, newlineIndex + 1);

                        if (!string.IsNullOrEmpty(message))
                        {
                            ProcessServerMessage(message);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                if (_isConnected)
                {
                    ErrorOccurred?.Invoke(this, $"接收消息失败: {ex.Message}");
                    HandleDisconnect();
                }
            }
        }

        /// <summary>
        /// 处理服务器消息
        /// </summary>
        private void ProcessServerMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("galaxy", out var galaxyProp) &&
                    root.TryGetProperty("message", out var messageProp))
                {
                    string galaxy = galaxyProp.GetString() ?? "未知";
                    string message = messageProp.GetString() ?? "";

                    MessageReceived?.Invoke(this, $"[{galaxy}] {message}");
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"解析消息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 心跳回调
        /// </summary>
        private async void HeartbeatCallback(object? state)
        {
            await SendHeartbeatAsync();
        }

        /// <summary>
        /// 处理断线
        /// </summary>
        private async void HandleDisconnect()
        {
            if (!_isConnected) return;

            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);

            // 清理当前连接
            Cleanup();

            // 尝试重连
            if (_reconnectAttempts < MaxReconnectAttempts)
            {
                await ReconnectAsync();
            }
        }

        /// <summary>
        /// 重连
        /// </summary>
        private async Task ReconnectAsync()
        {
            int delay = _reconnectDelays[Math.Min(_reconnectAttempts, _reconnectDelays.Length - 1)];
            _reconnectAttempts++;

            await Task.Delay(delay * 1000);

            try
            {
                bool success = await ConnectAsync(_serverIp, _serverPort, _username, _galaxy);
                if (success)
                {
                    _reconnectAttempts = 0; // 重连成功，重置计数
                }
                else if (_reconnectAttempts < MaxReconnectAttempts)
                {
                    await ReconnectAsync();
                }
            }
            catch
            {
                if (_reconnectAttempts < MaxReconnectAttempts)
                {
                    await ReconnectAsync();
                }
            }
        }

        /// <summary>
        /// 比较预警是否相同
        /// </summary>
        private bool AreAlertsEqual(Dictionary<ImageProcessor.AlertType, int> a, Dictionary<ImageProcessor.AlertType, int> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a.Count != b.Count) return false;

            foreach (var kvp in a)
            {
                if (!b.TryGetValue(kvp.Key, out var bValue)) return false;
                if (kvp.Value != bValue) return false;
            }

            return true;
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            try
            {
                _heartbeatTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _heartbeatTimer?.Dispose();
                _heartbeatTimer = null;

                _receiveCts?.Cancel();
                _receiveCts?.Dispose();
                _receiveCts = null;

                _networkStream?.Close();
                _tcpClient?.Close();
            }
            catch { }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            DisconnectAsync().Wait(1000);
            Cleanup();
        }

        /// <summary>
        /// 重置预警状态（用于重新开始检测时）
        /// </summary>
        public void ResetAlertState()
        {
            lock (_alertsLock)
            {
                _lastSentAlerts.Clear();
            }
        }
    }
}
