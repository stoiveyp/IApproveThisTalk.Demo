using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Messages.TextEntities;
using Slack.NetStandard.WebApi.Conversations;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/interaction")]
    [SlackAuth]
    public class C_BlockPayload : ControllerBase
    {
        private readonly ILogger<InteractionController> _logger;
        private readonly ISlackApiClient _webapi;

        public C_BlockPayload(ILogger<InteractionController> logger, ISlackApiClient webapi)
        {
            _logger = logger;
            _webapi = webapi;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public async Task<ActionResult> Post(InteractionPayload payload)
        {
            return payload switch
            {
                BlockActionsPayload blocks => await SendResult(blocks),
                _ => new OkObjectResult("Unsupported - sorry!")
            };
        }

        private async Task<ActionResult> SendResult(BlockActionsPayload blocks)
        {
            var decision = blocks.Actions.First().Value;
            string item = "item";
            string originator = "originator";

            var approverMessage = $"Request {UserMention.Text(originator)} made for {item} was {decision} by {UserMention.Text(blocks.User.ID)}";
            await new HttpClient().PostAsync(blocks.ResponseUrl, new StringContent(approverMessage));

            await _webapi.Conversations.Open(new ConversationOpenRequest
            {
                Users = originator
            });

            return new OkResult();
        }
    }
}