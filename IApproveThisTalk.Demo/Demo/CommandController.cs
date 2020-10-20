using System.Linq;
using Microsoft.AspNetCore.Http;
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
        [Consumes("application/x-www-form-urlencoded")]
        public ActionResult Post()
        {
            return new OkObjectResult("Unsupported - sorry!");
        }
    }
}
