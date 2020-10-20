using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard.Interaction;

namespace IApproveThisTalk.Demo.BreakGlass
{
    [ApiController]
    [Route("slack/a_slackcommand")]
    [SlackAuth]
    public class A_SlashCommand : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public ActionResult Post()
        {
            SlashCommand slashCommand = Request.Form.ToSlashCommand();

            return slashCommand.Command.Substring(1).ToLower() switch
            {
                "approve" => new OkObjectResult("Approved!"),
                _ => new OkObjectResult("Unsupported command - sorry!")
            };
        }
    }
}
