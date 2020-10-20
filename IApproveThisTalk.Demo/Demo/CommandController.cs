using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard.Interaction;

namespace IApproveThisTalk.Demo
{
    [ApiController]
    [Route("slack/commands")]
    [SlackAuth]
    public class CommandController:ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public ActionResult Post(SlashCommand slashCommand)
        {
            return new OkObjectResult("Unsupported - sorry!");
        }
    }
}
