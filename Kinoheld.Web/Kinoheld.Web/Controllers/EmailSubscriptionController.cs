using System.Threading.Tasks;
using Kinoheld.Application.Abstractions.RequestHandler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Kinoheld.Web.Controllers
{
    [Route("api/[controller]")]
    public class EmailSubscriptionController : Controller
    {
        private readonly IEmailSubscriptionHandler m_handler;

        public EmailSubscriptionController(IEmailSubscriptionHandler handler)
        {
            m_handler = handler;
        }

        [HttpGet]
        public IActionResult Get([FromQuery]string alexaId)
        {
            var content = "<html>" +
                          "<body>" +
                          "<form method='post'>" +
                          $"AlexaId: <input name='alexaId' value='{alexaId}' />" +
                          "<input type='submit' value='Unsubscribe' />" +
                          "</form>" +
                          "</body>" +
                          "</html>";
            return new ContentResult()
            {
                Content = content,
                ContentType = "text/html",
            };
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromQuery]string alexaId)
        {
            await m_handler.Unsubscribe(alexaId);

            var content = "<html>" +
                          "<body>" +
                          "Successfully unsubscibed." +
                          "</form>" +
                          "</body>" +
                          "</html>";
            return new ContentResult()
            {
                Content = content,
                ContentType = "text/html",
            };
        }
    }
}