using System.Drawing;
using System.Threading.Tasks;

namespace EVE预警
{
    public partial class Main : Form
    {
        private IntPtr currentWindowHandle = IntPtr.Zero;
        private System.Windows.Forms.Timer? captureTimer;
        private System.Windows.Forms.Timer? updateTimer;
        private int countdown = 3;
        private bool isProcessing = false;
        private bool isDetecting = false;
        private Bitmap? originalScreenshot; // 保存原始截图用于实时裁切预览

        // 滑动条优化相关
        private System.Windows.Forms.Timer? updateDebounceTimer; // 防抖定时器
        private bool updatePending = false; // 是否有待处理的更新

        // 网络连接相关
        private TcpClientManager? _tcpClientManager;
        private string? _currentUsername; // OCR识别的用户名
        private AppConfig _config; // 应用程序配置
        private Dictionary<ImageProcessor.AlertType, int> _lastAlerts = new(); // 上次预警状态

        public Main()
        {
            InitializeComponent();
            InitializeTimers();
            InitializeUpdateRate();
            InitializeOcr();
            InitializeNetwork();
            AddLog("=== 程序启动成功 ===");
            AddLog($"程序版本: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
            AddLog("当前时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            AddLog("=== 系统初始化完成 ===");
        }

        private void InitializeNetwork()
        {
            // 加载配置
            _config = AppConfig.Load();
            textBoxServerIP.Text = _config.ServerIp;

            // 初始化TCP管理器
            _tcpClientManager = new TcpClientManager();
            _tcpClientManager.MessageReceived += OnTcpMessageReceived;
            _tcpClientManager.ConnectionStateChanged += OnConnectionStateChanged;
            _tcpClientManager.ErrorOccurred += OnTcpError;
        }

        private void InitializeOcr()
        {
            try
            {
                OcrProcessor.Initialize();
                if (OcrProcessor.IsInitialized)
                {
                    AddLog("[OCR] OCR引擎初始化成功");
                }
                else
                {
                    AddLog($"[OCR] OCR引擎初始化失败: {OcrProcessor.InitError}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"[OCR] OCR引擎初始化异常: {ex.Message}");
            }
        }

        private void InitializeTimers()
        {
            AddLog("[初始化] 初始化定时器...");
            captureTimer = new System.Windows.Forms.Timer();
            captureTimer.Interval = 1000;
            captureTimer.Tick += CaptureTimer_Tick;

            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Tick += async (sender, e) => await UpdateTimer_TickAsync(sender, e);

            // 初始化防抖定时器
            updateDebounceTimer = new System.Windows.Forms.Timer();
            updateDebounceTimer.Interval = 50; // 50ms防抖间隔
            updateDebounceTimer.Tick += (sender, e) =>
            {
                updateDebounceTimer!.Stop();
                if (updatePending)
                {
                    updatePending = false;
                    UpdatePictureBoxDisplay();
                }
            };

            AddLog("[初始化] 定时器初始化完成");
        }

        private void InitializeUpdateRate()
        {
            comboBoxUpdateRate.SelectedIndex = 1;
            AddLog("[初始化] 更新频率默认设置为: 1秒");
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            if (captureTimer?.Enabled ?? false)
            {
                AddLog("[警告] 抓取操作正在进行中，请稍候");
                return;
            }

            if (isProcessing)
            {
                AddLog("[警告] 图像处理正在进行中，请稍候");
                return;
            }

            countdown = 3;
            buttonCapture.Text = $"抓取中... {countdown}";
            buttonCapture.Enabled = false;
            captureTimer?.Start();
            AddLog("[抓取] 开始3秒倒计时，请切换到目标窗口");
        }

        private void CaptureTimer_Tick(object? sender, EventArgs e)
        {
            countdown--;

            if (countdown > 0)
            {
                buttonCapture.Text = $"抓取中... {countdown}";
                AddLog($"[抓取] 倒计时: {countdown}秒");
            }
            else
            {
                captureTimer?.Stop();
                _ = CaptureForegroundWindowAsync();
                buttonCapture.Text = "抓取窗口";
                buttonCapture.Enabled = true;
            }
        }

        private async Task CaptureForegroundWindowAsync()
        {
            AddLog("[抓取] 开始获取前台窗口句柄...");
            try
            {
                currentWindowHandle = GetForegroundWindow();

                if (currentWindowHandle != IntPtr.Zero)
                {
                    string handleHex = currentWindowHandle.ToString("X");
                    labelHandle.Text = $"当前窗口句柄: {handleHex}";
                    AddLog($"[抓取] 成功获取窗口句柄: 0x{handleHex}");

                    await CaptureAndDisplayImageAsync(false);

                    if (!isDetecting)
                    {
                        AddLog("[抓取] 截图完成，请点击[开始检测]按钮开始色块检测");
                    }
                }
                else
                {
                    labelHandle.Text = "当前窗口句柄: 无";
                    AddLog("[抓取] 未能获取前台窗口句柄");
                }
            }
            catch (Exception ex)
            {
                AddLog($"[错误] 抓取窗口失败: {ex.Message}");
                AddLog($"[错误] 异常类型: {ex.GetType().Name}");
                AddLog($"[错误] 异常堆栈: {ex.StackTrace}");
            }
        }

        private async Task CaptureAndDisplayImageAsync(bool detectColors)
        {
            if (isProcessing)
            {
                AddLog("[警告] 跳过本次更新，上一次处理尚未完成");
                return;
            }

            isProcessing = true;
            AddLog("[处理] 开始异步处理图像...");
            DateTime startTime = DateTime.Now;

            int startX = 0, startY = 0, endX = 1920, endY = 1080;

            if (trackBarStartX.InvokeRequired)
            {
                trackBarStartX.Invoke((Action)(() =>
                {
                    startX = trackBarStartX.Value;
                    startY = trackBarStartY.Value;
                    endX = trackBarEndX.Value;
                    endY = trackBarEndY.Value;
                }));
            }
            else
            {
                startX = trackBarStartX.Value;
                startY = trackBarStartY.Value;
                endX = trackBarEndX.Value;
                endY = trackBarEndY.Value;
            }

            try
            {
                await Task.Run(() =>
                {
                    AddLog("[处理] 正在截取窗口图像...");
                    RECT rect = new RECT();
                    GetWindowRect(currentWindowHandle, ref rect);

                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    AddLog($"[处理] 窗口尺寸: {width}x{height}");

                    // 仅在抓取窗口时（非检测模式）更新UI：设置trackBar最大值和textBox值为窗口尺寸
                    if (!detectColors)
                    {
                        Invoke((Action)(() =>
                        {
                            trackBarEndX.Maximum = width;
                            trackBarEndY.Maximum = height;
                            textBoxEndX.Text = width.ToString();
                            textBoxEndY.Text = height.ToString();
                            trackBarEndX.Value = width;
                            trackBarEndY.Value = height;
                        }));
                    }

                    if (width > 0 && height > 0)
                    {
                        using (Bitmap windowImage = new Bitmap(width, height))
                        {
                            using (Graphics g = Graphics.FromImage(windowImage))
                            {
                                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(width, height));
                            }

                            AddLog("[处理] 窗口图像截取完成");

                            AddLog($"[处理] 裁切区域: ({startX},{startY}) - ({endX},{endY})");

                            if (startX >= 0 && startY >= 0 && endX > startX && endY > startY &&
                                endX <= windowImage.Width && endY <= windowImage.Height)
                            {
                                int cropWidth = endX - startX;
                                int cropHeight = endY - startY;

                                using (Bitmap croppedImage = new Bitmap(cropWidth, cropHeight))
                                {
                                    using (Graphics g = Graphics.FromImage(croppedImage))
                                    {
                                        g.DrawImage(windowImage, new Rectangle(0, 0, cropWidth, cropHeight),
                                                   new Rectangle(startX, startY, cropWidth, cropHeight),
                                                   GraphicsUnit.Pixel);
                                    }

                                    AddLog($"[处理] 成功裁切图像，尺寸: {cropWidth}x{cropHeight}");

                                    if (detectColors)
                                    {
                                        AddLog("[处理] 开始色块检测 (模板匹配)...");

                                        // 使用模板匹配检测色块
                                        var colorCounts = ImageProcessor.DetectAlertColorsFromTemplates(croppedImage);

                                        int totalDetected = 0;
                                        foreach (var kvp in colorCounts)
                                        {
                                            if (kvp.Value > 0)
                                            {
                                                totalDetected += kvp.Value;
                                            }
                                        }

                                        AddLog($"[处理] 色块检测完成，共检测到 {totalDetected} 个色块");

                                        if (totalDetected > 0)
                                        {
                                            foreach (var kvp in colorCounts)
                                            {
                                                if (kvp.Value > 0)
                                                {
                                                    string alertMessage = ImageProcessor.GetAlertMessage(kvp.Key);
                                                    AddLog($"[警报] {alertMessage} - 数量: {kvp.Value}");
                                                }
                                            }
                                            
                                            // 如果勾选了播放声音，则播放警报声音
                                            if (checkBoxPlaySound.Checked)
                                            {
                                                PlayAlertSound();
                                            }

                                            // 发送预警数据到服务器（如果已连接）
                                            if (_tcpClientManager?.IsConnected == true && !string.IsNullOrEmpty(textBoxGalaxy.Text))
                                            {
                                                _ = _tcpClientManager.SendAlertAsync(colorCounts, textBoxGalaxy.Text);
                                            }
                                        }
                                        else
                                        {
                                            AddLog("[处理] 未检测到警报色块");
                                        }
                                    }

                                    Invoke((Action)(() =>
                                    {
                                        if (pictureBox.Image != null)
                                        {
                                            pictureBox.Image.Dispose();
                                        }
                                        pictureBox.Image = (Image)croppedImage.Clone();

                                        // 保存原始截图用于实时裁切预览
                                        if (!detectColors)
                                        {
                                            originalScreenshot?.Dispose();
                                            originalScreenshot = (Bitmap)croppedImage.Clone();
                                        }
                                    }));
                                }
                            }
                            else
                            {
                                AddLog("[警告] 裁切区域参数无效，显示完整窗口截图");
                                Invoke((Action)(() =>
                                {
                                    pictureBox.Image = (Image)windowImage.Clone();
                                }));
                            }
                        }
                    }
                    else
                    {
                        AddLog("[警告] 窗口尺寸无效");
                    }
                });
            }
            catch (Exception ex)
            {
                AddLog($"[错误] 图像处理失败: {ex.Message}");
                AddLog($"[错误] 异常类型: {ex.GetType().Name}");
                AddLog($"[错误] 异常堆栈: {ex.StackTrace}");
            }
            finally
            {
                isProcessing = false;
                TimeSpan duration = DateTime.Now - startTime;
                AddLog($"[处理] 图像处理完成，耗时: {duration.TotalMilliseconds:F2}ms");
            }
        }

        private void buttonStartDetection_Click(object sender, EventArgs e)
        {
            if (currentWindowHandle == IntPtr.Zero)
            {
                AddLog("[警告] 请先抓取窗口，然后再开始检测");
                return;
            }

            if (isDetecting)
            {
                AddLog("[警告] 检测已经在运行中");
                return;
            }

            isDetecting = true;
            buttonStartDetection.Enabled = false;
            buttonStopDetection.Enabled = true;
            AddLog("[检测] 开始色块检测...");
            AddLog("[检测] 使用嵌入资源模板匹配模式");

            StartUpdateTimer();
        }

        private void buttonStopDetection_Click(object sender, EventArgs e)
        {
            if (!isDetecting)
            {
                AddLog("[警告] 检测尚未开始");
                return;
            }

            isDetecting = false;
            buttonStartDetection.Enabled = true;
            buttonStopDetection.Enabled = false;
            AddLog("[检测] 停止色块检测");

            StopUpdateTimer();
        }

        private void StartUpdateTimer()
        {
            double intervalSeconds = GetSelectedInterval();
            updateTimer?.Stop();
            updateTimer!.Interval = (int)(intervalSeconds * 1000);
            updateTimer.Start();
            AddLog($"[定时] 启动定时更新，频率: {intervalSeconds}秒");
        }

        private void StopUpdateTimer()
        {
            updateTimer?.Stop();
            AddLog("[定时] 停止定时更新");
        }

        private async Task UpdateTimer_TickAsync(object? sender, EventArgs e)
        {
            AddLog("[定时] 触发定时更新...");
            if (currentWindowHandle != IntPtr.Zero && isDetecting)
            {
                await CaptureAndDisplayImageAsync(true);
            }
            else
            {
                AddLog("[定时] 窗口句柄无效或未开始检测，跳过更新");
            }
        }

        private double GetSelectedInterval()
        {
            string selected = comboBoxUpdateRate.SelectedItem?.ToString() ?? "5秒";
            double interval = double.Parse(selected.Replace("秒", ""));
            AddLog($"[配置] 当前更新频率: {interval}秒");
            return interval;
        }

        private async void buttonExit_Click(object sender, EventArgs e)
        {
            AddLog("[退出] 准备退出程序...");
            StopUpdateTimer();
            
            // 断开网络连接
            if (_tcpClientManager?.IsConnected == true)
            {
                await _tcpClientManager.DisconnectAsync();
                _tcpClientManager.Dispose();
            }
            
            AddLog("[退出] 程序正在退出...");
            Application.Exit();
        }

        /// <summary>
        /// 连接服务器按钮点击事件 - 使用OCR识别用户名并连接服务器
        /// </summary>
        private async void buttonConnectSever_Click(object sender, EventArgs e)
        {
            if (originalScreenshot == null)
            {
                AddLog("[警告] 请先抓取窗口，然后再连接服务器");
                return;
            }

            if (!OcrProcessor.IsInitialized)
            {
                AddLog("[OCR] OCR引擎未初始化，无法识别");
                return;
            }

            string serverIp = textBoxServerIP.Text.Trim();
            string galaxy = textBoxGalaxy.Text.Trim();

            if (string.IsNullOrEmpty(serverIp))
            {
                AddLog("[警告] 请输入服务器IP地址");
                return;
            }

            if (string.IsNullOrEmpty(galaxy))
            {
                AddLog("[警告] 请输入星系名称");
                return;
            }

            // 如果已连接，先断开
            if (_tcpClientManager?.IsConnected == true)
            {
                await _tcpClientManager.DisconnectAsync();
            }

            AddLog("[OCR] 开始识别窗口标题区域文字...");

            try
            {
                // 异步执行OCR识别
                string? recognizedText = await Task.Run(() =>
                {
                    return OcrProcessor.RecognizeText(
                        originalScreenshot,
                        left: 40,
                        top: 0,
                        right: 400,
                        bottom: 36
                    );
                });

                if (string.IsNullOrEmpty(recognizedText))
                {
                    AddLog("[OCR] 未能识别到文字");
                    return;
                }

                AddLog($"[OCR] 识别到原始文字: {recognizedText}");

                // 按'-'拆分识别结果，提取'-'后边的部分作为角色名
                string[] parts = recognizedText.Split(new[] { '-' }, 2);
                if (parts.Length >= 2)
                {
                    string roleName = parts[1].Trim();

                    if (!string.IsNullOrEmpty(roleName))
                    {
                        AddLog($"识别到角色名：{roleName}");
                        _currentUsername = roleName;
                    }
                    else
                    {
                        AddLog("[OCR] 识别到'-'，但未找到角色名");
                        return;
                    }
                }
                else
                {
                    AddLog($"[OCR] 未找到'-'分隔符，识别内容为: {recognizedText}");
                    return;
                }

                // 连接服务器
                AddLog($"[网络] 正在连接服务器 {serverIp}:2424...");
                bool success = await _tcpClientManager!.ConnectAsync(serverIp, 2424, _currentUsername, galaxy);

                if (success)
                {
                    AddLog($"[网络] 成功连接到服务器 {serverIp}");
                    
                    // 保存配置
                    _config.ServerIp = serverIp;
                    AppConfig.Save(_config);

                    // 更新UI
                    connectSever.Enabled = false;
                    connectSever.Text = "已连接";
                    buttonDisconnectServer.Enabled = true;
                }
                else
                {
                    AddLog("[网络] 连接服务器失败");
                }
            }
            catch (Exception ex)
            {
                AddLog($"[OCR] 识别失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 断开服务器连接
        /// </summary>
        private async void buttonDisconnectServer_Click(object sender, EventArgs e)
        {
            if (_tcpClientManager?.IsConnected == true)
            {
                await _tcpClientManager.DisconnectAsync();
                AddLog("[网络] 已断开与服务器的连接");
            }

            connectSever.Enabled = true;
            connectSever.Text = "连接服务器";
            buttonDisconnectServer.Enabled = false;
        }

        /// <summary>
        /// TCP消息接收事件处理
        /// </summary>
        private void OnTcpMessageReceived(object? sender, string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object?, string>(OnTcpMessageReceived), sender, message);
                return;
            }

            // 解析并转换预警类型名为中文
            string displayMessage = TranslateAlertMessage(message);

            string logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] [广播] {displayMessage}";
            richTextBoxAlertLog.AppendText(logEntry + Environment.NewLine);
            richTextBoxAlertLog.ScrollToCaret();
        }

        /// <summary>
        /// 将预警消息中的英文类型名转换为中文
        /// 例如: "[晨曦] RedStar:2,OrangeCross:1" -> "[晨曦] 宣战:2,对战:1"
        /// </summary>
        private string TranslateAlertMessage(string message)
        {
            // 预警类型映射
            var alertTypeMap = new Dictionary<string, string>
            {
                { "RedStar", "宣战" },
                { "OrangeCross", "对战" },
                { "DarkRedSquare", "糟糕" },
                { "DarkOrangePlus", "不良" },
                { "GraySquare", "白名" }
            };

            // 解析消息格式: "[星系] 类型:数量,类型:数量"
            int bracketEnd = message.IndexOf(']');
            if (bracketEnd == -1)
            {
                // 如果没有星系前缀，直接转换
                return TranslateAlertTypes(message, alertTypeMap);
            }

            string prefix = message.Substring(0, bracketEnd + 1);
            string alerts = message.Substring(bracketEnd + 1).Trim();

            return prefix + " " + TranslateAlertTypes(alerts, alertTypeMap);
        }

        /// <summary>
        /// 转换预警类型名
        /// </summary>
        private string TranslateAlertTypes(string alerts, Dictionary<string, string> typeMap)
        {
            var parts = alerts.Split(',');
            var translated = new List<string>();

            foreach (var part in parts)
            {
                string trimmed = part.Trim();
                int colonIndex = trimmed.IndexOf(':');
                if (colonIndex > 0)
                {
                    string typeName = trimmed.Substring(0, colonIndex);
                    string count = trimmed.Substring(colonIndex + 1);

                    if (typeMap.TryGetValue(typeName, out string? chineseName))
                    {
                        translated.Add($"{chineseName}:{count}");
                    }
                    else
                    {
                        translated.Add(trimmed);
                    }
                }
                else
                {
                    translated.Add(trimmed);
                }
            }

            return string.Join(",", translated);
        }

        /// <summary>
        /// 连接状态变化事件处理
        /// </summary>
        private void OnConnectionStateChanged(object? sender, bool isConnected)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object?, bool>(OnConnectionStateChanged), sender, isConnected);
                return;
            }

            if (isConnected)
            {
                labelConnectionState.Text = "● 已连接";
                labelConnectionState.ForeColor = Color.Green;
            }
            else
            {
                labelConnectionState.Text = "● 未连接";
                labelConnectionState.ForeColor = Color.Gray;
                connectSever.Enabled = true;
                connectSever.Text = "连接服务器";
                buttonDisconnectServer.Enabled = false;
            }
        }

        /// <summary>
        /// TCP错误事件处理
        /// </summary>
        private void OnTcpError(object? sender, string error)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object?, string>(OnTcpError), sender, error);
                return;
            }

            AddLog($"[网络错误] {error}");
        }

        /// <summary>
        /// 播放警报声音
        /// </summary>
        private void PlayAlertSound()
        {
            try
            {
                // 使用系统警告声音
                Task.Run(() =>
                {
                    try
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                    }
                    catch
                    {
                        // 忽略播放错误
                    }
                });
            }
            catch
            {
                // 忽略初始化错误
            }
        }

        private void AddLog(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

            if (richTextBoxLog.InvokeRequired)
            {
                richTextBoxLog.Invoke((Action)(() =>
                {
                    richTextBoxLog.AppendText(logEntry + Environment.NewLine);
                    richTextBoxLog.ScrollToCaret();
                }));
            }
            else
            {
                richTextBoxLog.AppendText(logEntry + Environment.NewLine);
                richTextBoxLog.ScrollToCaret();
            }
        }

        private void trackBar_Scroll(object? sender, EventArgs e)
        {
            // 只更新被拖动的 trackBar 对应的 textBox
            if (sender == trackBarStartX)
            {
                textBoxStartX.Text = trackBarStartX.Value.ToString();
            }
            else if (sender == trackBarStartY)
            {
                textBoxStartY.Text = trackBarStartY.Value.ToString();
            }
            else if (sender == trackBarEndX)
            {
                textBoxEndX.Text = trackBarEndX.Value.ToString();
            }
            else if (sender == trackBarEndY)
            {
                textBoxEndY.Text = trackBarEndY.Value.ToString();
            }

            // 使用防抖机制优化平滑性
            updatePending = true;
            updateDebounceTimer?.Stop();
            updateDebounceTimer?.Start();
        }

        private void UpdatePictureBoxDisplay()
        {
            if (originalScreenshot == null)
                return;

            int startX = trackBarStartX.Value;
            int startY = trackBarStartY.Value;
            int endX = trackBarEndX.Value;
            int endY = trackBarEndY.Value;

            if (startX >= 0 && startY >= 0 && endX > startX && endY > startY &&
                endX <= originalScreenshot.Width && endY <= originalScreenshot.Height)
            {
                int cropWidth = endX - startX;
                int cropHeight = endY - startY;

                Bitmap croppedImage = new Bitmap(cropWidth, cropHeight);
                using (Graphics g = Graphics.FromImage(croppedImage))
                {
                    // 使用高质量插值算法提高图像质量
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                    g.DrawImage(originalScreenshot, new Rectangle(0, 0, cropWidth, cropHeight),
                               new Rectangle(startX, startY, cropWidth, cropHeight),
                               GraphicsUnit.Pixel);
                }

                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                }

                // 使用双缓冲技术避免闪烁
                pictureBox.Image = croppedImage;
                pictureBox.Invalidate(); // 触发重绘
            }
        }

        private void textBox_TextChanged(object? sender, EventArgs e)
        {
            int value;

            if (sender == textBoxStartX && int.TryParse(textBoxStartX.Text, out value))
            {
                trackBarStartX.Value = Math.Max(0, Math.Min(trackBarStartX.Maximum, value));
            }
            else if (sender == textBoxStartY && int.TryParse(textBoxStartY.Text, out value))
            {
                trackBarStartY.Value = Math.Max(0, Math.Min(trackBarStartY.Maximum, value));
            }
            else if (sender == textBoxEndX && int.TryParse(textBoxEndX.Text, out value))
            {
                trackBarEndX.Value = Math.Max(0, Math.Min(trackBarEndX.Maximum, value));
            }
            else if (sender == textBoxEndY && int.TryParse(textBoxEndY.Text, out value))
            {
                trackBarEndY.Value = Math.Max(0, Math.Min(trackBarEndY.Maximum, value));
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        private void richTextBoxLog_TextChanged(object sender, EventArgs e)
        {

        }

        #region 框选裁切功能

        /// <summary>
        /// 框选裁切按钮点击事件 - 打开新窗口进行框选
        /// </summary>
        private void buttonCropSelect_Click(object sender, EventArgs e)
        {
            if (originalScreenshot == null)
            {
                AddLog("[警告] 请先抓取窗口，然后再选择裁切区域");
                return;
            }

            AddLog("[框选] 打开裁选择窗口...");

            // 打开裁切选择窗口
            using (var cropForm = new CropSelectForm(originalScreenshot))
            {
                var result = cropForm.ShowDialog(this);

                if (result == DialogResult.OK && cropForm.Confirmed)
                {
                    // 更新裁切参数
                    int startX = cropForm.StartX;
                    int startY = cropForm.StartY;
                    int endX = cropForm.EndX;
                    int endY = cropForm.EndY;

                    // 更新trackBar
                    trackBarStartX.Value = startX;
                    trackBarStartY.Value = startY;
                    trackBarEndX.Value = endX;
                    trackBarEndY.Value = endY;

                    // 更新textBox
                    textBoxStartX.Text = startX.ToString();
                    textBoxStartY.Text = startY.ToString();
                    textBoxEndX.Text = endX.ToString();
                    textBoxEndY.Text = endY.ToString();

                    AddLog($"[框选] 已设置裁切区域: ({startX},{startY}) - ({endX},{endY})");
                    AddLog($"[框选] 裁切尺寸: {endX - startX}x{endY - startY}");

                    // 更新裁切预览
                    UpdatePictureBoxDisplay();
                }
                else
                {
                    AddLog("[框选] 用户取消了裁切选择");
                }
            }
        }

        #endregion

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}