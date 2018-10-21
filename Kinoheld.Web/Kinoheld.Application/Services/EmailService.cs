using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Kinoheld.Application.Model;
using Kinoheld.Application.ResponseMessages;
using Kinoheld.Base;
using Microsoft.Extensions.Configuration;

namespace Kinoheld.Application.Services
{
    public class EmailService : IEmailService
    {
        private const string RecepientTestEmail = "trash123cin@trash-mail.com";
        private readonly IMessages m_messages;
        private readonly IConfiguration m_configuration;

        public EmailService(
            IMessages messages,
            IConfiguration configuration)
        {
            m_messages = messages;
            m_configuration = configuration;
        }

        public async Task SendEmailOverviewAsync(string email, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(subject) ||
                string.IsNullOrEmpty(htmlBody))
            {
                return;
            }

            var details = GetSmtpDetails();
            var from = new MailAddress(details.FromAddress, details.FromAddress, Encoding.ASCII);
            var to = new MailAddress(email);
            using (var mail = CreateMessage(from, to, subject, htmlBody))
            {
                using (var smtpClient = CreateSmtpClient(details))
                {
                    await smtpClient.SendMailAsync(mail).ConfigureAwait(false);
                }
            }
        }

        private SmtpDetails GetSmtpDetails()
        {
            var smtpServer = GetFromConfiguration(Secrets.SmtpServer);
            var smtpPortStr = GetFromConfiguration(Secrets.SmtpPort);
            var fromAddress = GetFromConfiguration(Secrets.SmtpFrom);
            var fromPassword = GetFromConfiguration(Secrets.SmtpPass);
            if (!int.TryParse(smtpPortStr, out var smtpPort))
            {
                throw new ApplicationException("The given port has an invalid format.");
            }

            return new SmtpDetails(smtpServer, smtpPort, fromAddress, fromPassword);
        }

        private string GetFromConfiguration(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var val = m_configuration[name];
            if (string.IsNullOrEmpty(val))
            {
                throw new ApplicationException(
                    $"The secret key \"{name}\" has to be set properly.");
            }

            return val;
        }

        private SmtpClient CreateSmtpClient(SmtpDetails details, bool useSsl = true)
        {
            var smtpClient = new SmtpClient(details.SmtpServer, details.SmtpPort)
            {
                EnableSsl = useSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(details.FromAddress, details.FromPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            return smtpClient;
        }

        private MailMessage CreateMessage(MailAddress fromAddress, MailAddress toAddress, string subject, string body, bool isBodyHtml = true)
        {
            var mail = new MailMessage
            {
                From = fromAddress,
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };
            mail.To.Add(toAddress);
            return mail;
        }
        
        public async Task TestConnectionAsync()
        {
            var details = GetSmtpDetails();
            var from = new MailAddress(details.FromAddress, details.FromAddress, Encoding.ASCII);
            var to = new MailAddress(RecepientTestEmail);

            using (var mail = CreateMessage(from, to, "Email Test", "This is a test email sent.", false))
            {
                using (var smtpClient = CreateSmtpClient(details))
                {
                    await smtpClient.SendMailAsync(mail).ConfigureAwait(false);
                }
            }
        }
    }
}