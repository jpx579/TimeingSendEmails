using Microsoft.Win32;
using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace TimeingSendEmails
{
    public partial class Form1 : Form
    {
        private Timer _timer;
        private AppConfig _config;
        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenu;
        public Form1()
        {
            InitializeComponent();
            Form1_Load(null, null);

            // 创建 NotifyIcon
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Application; // 使用应用程序图标，您可以更换为您自己的图标
            notifyIcon.Text = "My Application";
            notifyIcon.Visible = true;

            // 创建右键菜单
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Restore", null, Restore);
            contextMenu.Items.Add("Exit", null, Exit);

            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.DoubleClick += Restore;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        private void Restore(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void Exit(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                notifyIcon.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfig();
            // 确保程序在开机时自动启动
            SetStartup();
            // 立即发送一次邮件
            RunFaceDetectionAndSendEmail();
            // 设置每小时发送一次邮件
            _timer = new Timer();
            _timer.Interval = _config.Interval; // 1小时
            _timer.Tick += Timer_Tick;
            _timer.Start();
            // 关机事件处理
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
        }
        private void LoadConfig()
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
        private void Timer_Tick(object sender, EventArgs e)
        {
            RunFaceDetectionAndSendEmail();
        }
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            RunFaceDetectionAndSendEmail();
        }
        private void RunFaceDetectionAndSendEmail()
        {
            try
            {
                FaceDetection faceDetection = new FaceDetection();
                EmailSender emailSender = new EmailSender();
                (bool faceDetected, string filePath) = faceDetection.DetectFace();
                if (faceDetected)
                {
                    emailSender.SendEmail("连培旭，同学！", "现在我在电脑前工作，你也要加油哦！", _config, filePath);
                }
                else
                {
                    emailSender.SendEmail("连培旭，同学！", "现在我没在电脑前，可以视频联系我哈！", _config, filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunFaceDetectionAndSendEmail: {ex.Message}");
            }
        }
        private void SetStartup()
        {
            string runKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(runKey, true))
            {
                key.SetValue("TimeingSendEmails", "\"" + Application.ExecutablePath + "\"");
            }
        }
    }
}
