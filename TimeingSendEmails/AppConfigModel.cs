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

        // 新增字段
        /// <summary>
        /// 电脑有人操作时的描述
        /// </summary>
        public string AtComputerDescription { get; set; } = "现在我在电脑前工作，你也要加油哦！";

        /// <summary>
        /// 电脑无人操作时的描述
        /// </summary>
        public string NotAtComputerDescription { get; set; } = "现在我没在电脑前，可以视频联系我哈！";
    }
}
