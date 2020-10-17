using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Messages;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Messages.TextEntities;
using Slack.NetStandard.WebApi.Chat;
using Slack.NetStandard.WebApi.Conversations;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/c_blockpayload")]
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
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult> Post([FromForm]string payload)
        {
            return JsonConvert.DeserializeObject<InteractionPayload>(payload) switch
            {
                BlockActionsPayload blocks => await SendResult(blocks),
                _ => new OkObjectResult("Unsupported - sorry!")
            };
        }

        private async Task<ActionResult> SendResult(BlockActionsPayload blocks)
        {
            var originatorBlock = blocks.Message.Blocks.OfType<Section>().First(b => b.BlockId == "originator");
            var itemBlock = blocks.Message.Blocks.OfType<Section>().First(b => b.BlockId == "request");

            var originator = TextParser.FindEntities(originatorBlock.Fields.Last().Text).First() as UserMention;
            var item = itemBlock.Fields.Last().Text;
            var decision = blocks.Actions.First().Value;

            var approverMessage = $"Request {UserMention.Text(originator.UserId)} made for {item} was {decision} by {UserMention.Text(blocks.User.ID)}";
            var message = new Message{Text=approverMessage};
            await new HttpClient().PostAsync(blocks.ResponseUrl, new StringContent(JsonConvert.SerializeObject(message),Encoding.UTF8,"application/json"));

            var conversation = await _webapi.Conversations.Open(new ConversationOpenRequest
            {
                Users = originator.UserId
            });
            await _webapi.Chat.Post(new PostMessageRequest
            {
                Channel = conversation.Channel.ID,
                Text = $"Your request, made for {item}, was {decision} by {UserMention.Text(blocks.User.ID)}"
            });
            return new OkResult();
        }
    }
}