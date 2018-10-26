using System.Threading.Tasks;
using Kinoheld.Application.Abstractions.RequestHandler;
using Kinoheld.Domain.Services.Abstractions.Database;

namespace Kinoheld.Application.RequestHandler
{
    public class EmailSubscriptionHandler : IEmailSubscriptionHandler
    {
        private readonly IKinoheldDbAccess m_dbAccess;

        public EmailSubscriptionHandler(IKinoheldDbAccess dbAccess)
        {
            m_dbAccess = dbAccess;
        }

        public async Task Unsubscribe(string alexaId)
        {
            await m_dbAccess.UnsubscribeEmail(alexaId).ConfigureAwait(false);
        }
    }
}