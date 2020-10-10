using IApproveThisTalk.Demo.Demo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Slack.NetStandard.Interaction;

namespace IApproveThisTalk.Demo.BreakGlass
{
    [ApiController]
    [Route("breakglass/A")]
    public class A_SlashCommand:ControllerBase
    {

        private readonly ILogger<InteractionController> _logger;

        public A_SlashCommand(ILogger<InteractionController> logger)
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
