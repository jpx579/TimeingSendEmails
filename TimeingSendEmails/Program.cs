using Microsoft.Win32;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace TimeingSendEmails
{
    internal static class Program
    {
        private static FaceDetection _faceDetection;
        private static EmailSender _emailSender;
        private static AppConfig _config;
        private static NotifyIcon notifyIcon;
        private static Timer _timer;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            _faceDetection = new FaceDetection();
            _emailSender = new EmailSender();
            Init();
            string icoPath = $"{Application.StartupPath}favicon.ico";
            notifyIcon = new NotifyIcon
            {
                Text = "TimeingSendEmailsApp",
                Icon = new Icon(icoPath),
                Visible = true
            };

            // 创建右键菜单
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, Exit);
            notifyIcon.ContextMenuStrip = contextMenu;

            // 设置定时器
            _timer = new Timer
            {
                Interval = _config.Interval * 1000  // 以秒为单位
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            Application.Run();
        }

        private static void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }
        private static void Init()
        {
            LoadConfig();
            // 确保程序在开机时自动启动
            SetStartup();
            // 立即发送一次邮件
            RunFaceDetectionAndSendEmail("电脑开机");
            // 关机事件处理
            SystemEvents.SessionEnding += new SessionEndingEventHandler(TurnOffAndSendEmailsEvents);
        }
        private static void LoadConfig()
        {
            string configPath = Application.StartupPath + "Config\\AppConfig.json";
            if (File.Exists(configPath))
            {
                string configJson = File.ReadAllText(configPath);
                _config = JsonConvert.DeserializeObject<AppConfig>(configJson);
            }
            else
            {
                throw new FileNotFoundException("不存在config文件！");
            }
        }
        private static void Timer_Tick(object sender, EventArgs e)
        {
            RunFaceDetectionAndSendEmail();
        }
        private static void TurnOffAndSendEmailsEvents(object sender, SessionEndingEventArgs e)
        {
            RunFaceDetectionAndSendEmail("电脑关机");
        }
        private static async void RunFaceDetectionAndSendEmail(string msg = "正在工作")
        {
            try
            {
                (bool faceDetected, string filePath) = _faceDetection.DetectFace();
                if (faceDetected)
                {
                    _emailSender.SendEmail("连培旭，同学！", $"【{msg}】现在我在电脑前工作，你也要加油哦！", _config, filePath);
                }
                else
                {
                    _emailSender.SendEmail("连培旭，同学！", $"【{msg}】现在我没在电脑前，可以视频联系我哈！", _config, filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunFaceDetectionAndSendEmail: {ex.Message}");
            }
            finally { GC.Collect(); }
        }
        private static void SetStartup()
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true))
            {
                key.SetValue("TimeingSendEmails", "\"" + Application.ExecutablePath + "\"");
            }
        }
    }
}