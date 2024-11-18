using System.Net;
using System.Net.Mail;

namespace TimeingSendEmails
{
    public class EmailSender
    {
        public void SendEmail(string subject, string body, AppConfig appConfig, string attachmentFilePath = null)
        {
            try
            {
                var fromAddress = new MailAddress(appConfig.FromEmail, appConfig.FromName);
                var toAddress = new MailAddress(appConfig.ToEmail, appConfig.ToName);
                string fromPassword = appConfig.AuthorizationCode;
                var smtp = new SmtpClient
                {
                    Host = "smtp.qq.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 3000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
                    {
                        message.Attachments.Add(new Attachment(attachmentFilePath)); // 添加附件
                    }

                    smtp.Send(message);
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendEmail: {ex.Message}");
            }
        }
    }

    public class AppConfig
    {
        /// <summary>
        /// 收件人邮箱
        /// </summary>
        public string ToEmail { get; set; }
        /// <summary>
        /// 收件人名称
        /// </summary>
        public string ToName { get; set; }
        /// <summary>
        /// 发送人邮箱
        /// </summary>
        public string FromEmail { get; set; }
        /// <summary>
        /// 发送人名称
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// 邮箱授权码
        /// </summary>
        public string AuthorizationCode { get; set; }
        /// <summary>
        /// 定时时间
        /// </summary>
        public int Interval { get; set; }
    }
}
