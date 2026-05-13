namespace TimeingSendEmails
{
    public static class Logger
    {
        private static string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Info(string message) => Write("INFO", message);
        public static void Error(string message, Exception ex = null)
            => Write("ERROR", $"{message} {(ex != null ? "\n" + ex.ToString() : "")}");

        private static void Write(string level, string message)
        {
            try
            {
                if (!Directory.Exists(_logDir)) Directory.CreateDirectory(_logDir);
                string path = Path.Combine(_logDir, $"{DateTime.Now:yyyy-MM-dd}.log");
                string content = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

                Console.Write(content);
                File.AppendAllText(path, content);
            }
            catch {}
        }
    }
}
