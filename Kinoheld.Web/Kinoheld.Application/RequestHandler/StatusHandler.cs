using System;
using System.Text;
using System.Threading.Tasks;
using Kinoheld.Application.Abstractions.RequestHandler;
using Kinoheld.Application.Services;
using Kinoheld.Domain.Services.Abstractions.Database;

namespace Kinoheld.Application.RequestHandler
{
    public class StatusHandler : IStatusHandler
    {
        private readonly IKinoheldDbAccess m_dbAccess;
        private readonly IEmailService m_emailService;

        public StatusHandler(IKinoheldDbAccess dbAccess, IEmailService emailService)
        {
            m_dbAccess = dbAccess;
            m_emailService = emailService;
        }

        public async Task<string> GetStatusAsync()
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("WebService OK");

            try
            {
                await m_dbAccess.TestConnectionAsync().ConfigureAwait(false);
                strBuilder.AppendLine("DB connection OK");
            }
            catch (Exception e)
            {
                strBuilder.AppendLine($"DB connection ERROR: {e}");
            }

            try
            {
                await m_emailService.TestConnectionAsync().ConfigureAwait(false);
                strBuilder.AppendLine("Email connection OK");
            }
            catch (Exception e)
            {
                strBuilder.AppendLine($"Email connection ERROR: {e}");
            }

            strBuilder.AppendLine(string.Empty);
            strBuilder.AppendLine("-----------");
            strBuilder.AppendLine($"TimeZoneInfo: {TimeZoneInfo.Local.StandardName}");
            strBuilder.AppendLine($"Time (Local): {DateTime.Now}");
            strBuilder.AppendLine($"Time (UTC): {DateTime.UtcNow}");
            strBuilder.AppendLine("-----------");


            return strBuilder.ToString();
        }
    }
}