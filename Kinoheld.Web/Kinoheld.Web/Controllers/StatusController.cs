using System.Threading.Tasks;
using Kinoheld.Application.Abstractions.RequestHandler;
using Microsoft.AspNetCore.Mvc;

namespace Kinoheld.Web.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        private readonly IStatusHandler m_handler;

        public StatusController(IStatusHandler handler)
        {
            m_handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var status = await m_handler.GetStatusAsync();
            return Ok(status);
        }
    }
}