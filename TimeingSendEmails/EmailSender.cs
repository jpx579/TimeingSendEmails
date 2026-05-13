using MimeKit;
using System.Net;
using System.Net.Mail;

namespace TimeingSendEmails
{
    public class EmailSender
    {
        /// <summary>
        /// 异步发送邮件（带自动重试机制）
        /// 默认先ipv4 在ipv6
        /// </summary>
        public async Task SendEmailAsync(string subject, string body, AppConfigModel appConfig, string attachmentFilePath = null)
        {
            try
            {
                Logger.Info($"准备发送邮件 (SmtpClient): {subject}");

                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var fromAddress = new MailAddress(appConfig.FromEmail, appConfig.FromName);
                var toAddress = new MailAddress(appConfig.ToEmail, appConfig.ToName);

                using (var smtp = new System.Net.Mail.SmtpClient
                {
                    Host = "smtp.qq.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, appConfig.AuthorizationCode),
                    Timeout = 15000 
                })
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                })
                {
                    if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
                    {
                        message.Attachments.Add(new Attachment(attachmentFilePath));
                    }

                    await smtp.SendMailAsync(message).ConfigureAwait(false);
                    Logger.Info("使用 SmtpClient IPV6 发送成功！");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"SmtpClient IPV6 发送失败，尝试使用 MailKit 备选方案。错误: {ex.Message}");

                await SendEmailWithMailKitAsync(subject, body, appConfig, attachmentFilePath);
            }
        }

        /// <summary>
        /// 备选方案：使用 MailKit 发送 (465端口，更稳定)
        /// </summary>
        public async Task SendEmailWithMailKitAsync(string subject, string body, AppConfigModel appConfig, string attachmentFilePath = null)
        {
            try
            {
                Logger.Info("启动 MailKit 备选发送方案 (Port 465)...");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(appConfig.FromName, appConfig.FromEmail));
                message.To.Add(new MailboxAddress(appConfig.ToName, appConfig.ToEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { TextBody = body };
                if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
                {
                    builder.Attachments.Add(attachmentFilePath);
                }
                message.Body = builder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync("smtp.qq.com", 465, true);
                    await client.AuthenticateAsync(appConfig.FromEmail, appConfig.AuthorizationCode);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Logger.Info("使用 MailKit 发送成功！");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("MailKit 备选方案也失败了，请检查网络或授权码。", ex);
            }
        }

        public async Task SendEmailAsync_IPV4(string subject, string body, AppConfigModel appConfig, string attachmentFilePath = null)
        {
            try
            {
                Logger.Info($"准备发送邮件 (强制针对 IPv4 进行优化): {subject}");

                var addresses = await Dns.GetHostAddressesAsync("smtp.qq.com");

                var ipv4Address = addresses.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ipv4Address == null)
                {
                    Logger.Error("无法解析到有效的 IPv4 地址，尝试直接使用域名。");
                }

                ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var fromAddress = new MailAddress(appConfig.FromEmail, appConfig.FromName);
                var toAddress = new MailAddress(appConfig.ToEmail, appConfig.ToName);

                using (var smtp = new System.Net.Mail.SmtpClient
                {
                    Host = ipv4Address?.ToString() ?? "smtp.qq.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, appConfig.AuthorizationCode),
                    Timeout = 10000 
                })
                {
                    smtp.TargetName = "STARTTLS/smtp.qq.com";

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    })
                    {
                        if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
                        {
                            message.Attachments.Add(new Attachment(attachmentFilePath));
                        }

                        await smtp.SendMailAsync(message).ConfigureAwait(false);
                        Logger.Info($"使用 SmtpClient (IPv4: {ipv4Address}) 发送成功！");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"IPv4 方案失败，切换到备选IPV6方案。错误: {ex.Message}");
                await SendEmailAsync(subject, body, appConfig, attachmentFilePath);
            }
        }
    }
}