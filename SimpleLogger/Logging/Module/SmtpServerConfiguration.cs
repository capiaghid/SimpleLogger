namespace SimpleLogger.Logging.Module
{
    public class SmtpServerConfiguration
    {
        public SmtpServerConfiguration(string userName, string password, string host, int port)
        {
            UserName = userName;
            Password = password;
            Host     = host;
            Port     = port;
        }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }
    }
}