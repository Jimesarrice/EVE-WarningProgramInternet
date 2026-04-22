using System.Text.Json;

namespace EVE预警
{
    /// <summary>
    /// 应用程序配置管理
    /// </summary>
    public class AppConfig
    {
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 2424;
        public string LastConnectedUsername { get; set; } = "";
        public bool AutoReconnect { get; set; } = true;

        private static string GetConfigPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "EVE预警");
            
            if (!Directory.Exists(configDir))
            {
                try
                {
                    Directory.CreateDirectory(configDir);
                }
                catch
                {
                    // 如果创建失败，使用exe目录
                    string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    return Path.Combine(exeDir, "config.json");
                }
            }

            return Path.Combine(configDir, "config.json");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public static AppConfig Load()
        {
            try
            {
                string configPath = GetConfigPath();
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    return config ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置文件失败: {ex.Message}");
            }

            return new AppConfig();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save(AppConfig config)
        {
            try
            {
                string configPath = GetConfigPath();
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }
    }
}
