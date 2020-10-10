using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Slack.NetStandard.Interaction;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/interaction")]
    [SlackAuth]
    public class InteractionController : ControllerBase
    {
        private readonly ILogger<InteractionController> _logger;

        public InteractionController(ILogger<InteractionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public ActionResult Post(InteractionPayload payload)
        {
            return payload switch
            {
                _ => new OkObjectResult("Unsupported - sorry!")
            };
        }
    }
}
