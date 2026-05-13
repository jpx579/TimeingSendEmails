TimeingSendEmails (同学监测助手) 🚀
这是一个基于 .NET 6.0 开发的 Windows 后台监测工具。它能通过摄像头定时进行人脸检测，并根据检测结果（是否在位）自动向指定的同学发送提醒邮件。

🌟 核心功能
人脸检测：利用 OpenCV (OpenCvSharp) 驱动摄像头，智能识别屏幕前是否有人。

定时提醒：根据配置的时间间隔（如每 6 分钟）自动执行检测与发送。

双重发送保障：

方案 A (IPv4 优化)：优先尝试 SmtpClient 通过 587 端口发送。

方案 B (MailKit 强力备选)：若方案 A 失败（如网络延迟、IPv6 干扰），自动切换到 MailKit 通过 465 端口加密发送。

开机自启：程序启动后自动写入注册表，实现随电脑开机自动运行。

系统托盘化：无窗口运行，通过右键托盘图标可进行“手动检测”或“退出程序”。

详细日志：自动在 Logs 文件夹生成按天滚动的日志文件，方便排查所有连接和发送细节。

🛠️ 环境要求
操作系统：Windows 10/11

硬件：带摄像头的电脑。

运行环境：.NET 6.0 Runtime

邮箱配置：发件人需开启 QQ 邮箱（或类似邮箱）的 SMTP 服务 并获取 16 位授权码。

📂 目录结构说明
Config/：存放 AppConfig.json（核心配置）和 haarcascade_frontalface_default.xml（人脸识别模型）。

FaceImages/：程序运行时自动创建，保存每次检测时的现场照片。

Logs/：存放程序运行日志，记录所有的检测结果和邮件发送状态。

favicon.ico：程序托盘图标。

⚙️ 配置文件指南 (Config/AppConfig.json)
在运行程序前，请务必修改配置文件中的邮箱信息。请注意：邮箱格式必须准确，且不能有多余空格。

JSON
{
  "ToEmail": "student@qq.com",      // 收件人邮箱（同学的）
  "ToName": "连同学",                // 收件人称呼
  "FromEmail": "your_me@qq.com",    // 你的发件人邮箱
  "FromName": "监测助手",            // 发件人显示名称
  "AuthorizationCode": "abcd...xyz", // 16位QQ邮箱授权码 (不是密码)
  "Interval": 360                   // 监测间隔（单位：秒），360即为6分钟
  "AtComputerDescription": "现在我在电脑前工作，你也要加油哦！", 
  "NotAtComputerDescription": "现在我没在电脑前，可以视频联系我哈！"
}
🚀 常见问题排查 (FAQ)
1. 邮件发送很慢（约 45 秒）？
这是由于 Windows 尝试通过 IPv6 连接造成的。程序已内置 IPv4 优先逻辑和 MailKit 备选方案。如果依然较慢，请确保网络环境稳定。

2. 提示 "The specified string is not in the form required for an e-mail address"？
这说明 AppConfig.json 里的 FromEmail 或 ToEmail 填写的格式不对。请检查是否有漏掉 @、多打空格或者 JSON 字段名拼写错误。

3. 摄像头打不开？
请检查是否有其他软件（如微信视频、会议软件）正在占用摄像头。

4. 为什么收不到邮件？
检查 授权码 是否过期或输入错误。

检查 QQ 邮箱设置中是否真的开启了 SMTP 服务。

查看 Logs/ 目录下的最新日志文件，里面记录了详细的报错原因。

📝 开发者备注
本程序主要用于技术交流与提醒同学学习。请勿用于非法监控他人的行为。

如何使用？
发布程序并解压。

修改 Config/AppConfig.json。

双击运行 TimeingSendEmails.exe。

在右下角系统托盘看到图标即代表启动成功。
