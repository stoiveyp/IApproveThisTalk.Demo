using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Messages.Elements;
using Slack.NetStandard.Objects;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/interaction")]
    [SlackAuth]
    public class InteractionController : ControllerBase
    {
        private readonly ILogger<InteractionController> _logger;
        private readonly ISlackApiClient _webapi;

        public InteractionController(ILogger<InteractionController> logger, ISlackApiClient client)
        {
            _logger = logger;
            _webapi = client;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult> Post([FromForm] string payload)
        {
            return JsonConvert.DeserializeObject<InteractionPayload>(payload) switch
            {
                _ => new OkObjectResult("Unsupported - sorry!")
            };
        }
    }
}