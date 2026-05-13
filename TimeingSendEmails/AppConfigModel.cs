namespace TimeingSendEmails
{
    public class AppConfigModel
    {
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 收件人邮箱
        /// </summary>
        public string ToEmail { get; set; } = string.Empty;

        /// <summary>
        /// 收件人名称
        /// </summary>
        public string ToName { get; set; } = string.Empty;

        /// <summary>
        /// 发送人邮箱
        /// </summary>
        public string FromEmail { get; set; } = string.Empty;

        /// <summary>
        /// 发送人名称
        /// </summary>
        public string FromName { get; set; } = string.Empty;

        /// <summary>
        /// 邮箱授权码
        /// </summary>
        public string AuthorizationCode { get; set; } = string.Empty;

        /// <summary>
        /// 定时时间
        /// </summary>
        public int Interval { get; set; } = 1;

    }
}
