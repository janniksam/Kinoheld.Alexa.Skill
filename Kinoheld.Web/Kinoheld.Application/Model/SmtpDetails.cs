namespace Kinoheld.Application.Model
{
    internal class SmtpDetails
    {
        public SmtpDetails(string smtpServer, int smtpPort, string fromAddress, string fromPassword)
        {
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            FromAddress = fromAddress;
            FromPassword = fromPassword;
        }

        public string SmtpServer { get; }

        public int SmtpPort { get; }

        public string FromAddress { get; }

        public string FromPassword { get; }
    }
}