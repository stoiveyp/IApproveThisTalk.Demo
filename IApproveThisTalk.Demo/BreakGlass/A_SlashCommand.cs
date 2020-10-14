using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard.Interaction;

namespace IApproveThisTalk.Demo.BreakGlass
{
    [ApiController]
    [Route("slack/temp_A")]
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
            SlashCommand slashCommand;
            string text = string.Empty;
            using (var sr = new StreamReader(Request.Body))
            {
                text = sr.ReadToEnd();
                slashCommand = new SlashCommand(text);
            }

            return slashCommand.Command.Substring(1).ToLower() switch
            {
                "approve" => new OkObjectResult("Approved!"),
                _ => new OkObjectResult("Unsupported command - sorry!")
            };
        }
    }
}
