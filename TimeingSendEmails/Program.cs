using Microsoft.Win32;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace TimeingSendEmails
{
    internal static class Program
    {
        private static FaceDetection _faceDetection = new FaceDetection();
        private static EmailSender _emailSender = new EmailSender();
        private static AppConfigModel _config = new AppConfigModel();
        private static NotifyIcon _notifyIcon = new NotifyIcon();
        private static Timer _timer = new Timer();
        private static bool _isProcessing = false; // 防止重入锁

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logger.Info("程序启动...");

            try
            {
                Init();
            }
            catch (Exception ex)
            {
                Logger.Error("初始化失败", ex);
                MessageBox.Show("初始化失败，请检查配置和日志。");
                return;
            }

            SetupNotifyIcon();

            _timer = new Timer
            {
                Interval = Math.Max(_config.Interval, 1) * 1000
            };
            _timer.Tick += async (s, e) => await RunTaskWrapper();
            _timer.Start();

            Logger.Info($"定时器已启动，间隔: {_config.Interval}秒");
            Application.Run();
        }

        private static void SetupNotifyIcon()
        {
            string icoPath = Path.Combine(Application.StartupPath, "favicon.ico");
            _notifyIcon = new NotifyIcon
            {
                Text = "邮件定时监测程序",
                Icon = File.Exists(icoPath) ? new Icon(icoPath) : SystemIcons.Application,
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("立即检测", null, async (s, e) => await RunTaskWrapper("手动触发"));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("退出", null, Exit);
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private static void Init()
        {
            LoadConfig();
            SetStartup();

            SystemEvents.SessionEnding += TurnOffAndSendEmailsEvents;

            _ = Task.Run(() => RunTaskWrapper("电脑开机"));
        }

        private static void LoadConfig()
        {
            string configPath = Path.Combine(Application.StartupPath, "Config", "AppConfig.json");
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"找不到配置文件: {configPath}");
            }

            string configJson = File.ReadAllText(configPath);
            _config = JsonConvert.DeserializeObject<AppConfigModel>(configJson) ?? throw new Exception("配置文件解析失败");
            Logger.Info("配置加载成功。");
        }

        private static async Task RunTaskWrapper(string msg = "正在工作")
        {
            if (_isProcessing)
            {
                Logger.Info("上次任务尚未结束，跳过本次触发。");
                return;
            }

            _isProcessing = true;
            try
            {
                Logger.Info($"开始检测流程: {msg}");

                (bool faceDetected, string filePath) = _faceDetection.DetectFace();

                string subject = _config.Title;
                string body = faceDetected
                    ? $"【{msg}】{_config.AtComputerDescription}"
                    : $"【{msg} {filePath}】{_config.NotAtComputerDescription}";

                Logger.Info(faceDetected ? "检测到人脸，准备发送正面邮件。" : "未检测到人脸，准备发送离座邮件。");

                await _emailSender.SendEmailAsync_IPV4(subject, body, _config, filePath);

                Logger.Info("邮件发送成功。");
            }
            catch (Exception ex)
            {
                Logger.Error("任务执行过程中出错", ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private static void TurnOffAndSendEmailsEvents(object sender, SessionEndingEventArgs e)
        {
            Logger.Info("接收到关机信号...");
            RunTaskWrapper("电脑关机").GetAwaiter().GetResult();
        }

        private static void SetStartup()
        {
            try
            {
                string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true)??throw new Exception("获取开机自启动注册表失败"))
                {
                    string path = $"\"{Application.ExecutablePath}\"";
                    key.SetValue("TimeingSendEmails", path);
                }
                Logger.Info("开机自启注册表检查完成。");
            }
            catch (Exception ex)
            {
                Logger.Error("设置自启失败（可能缺少权限）", ex);
            }
        }

        private static void Exit(object sender, EventArgs e)
        {
            Logger.Info("程序退出。");
            _notifyIcon.Visible = false;
            _timer.Stop();
            Application.Exit();
        }
    }
}