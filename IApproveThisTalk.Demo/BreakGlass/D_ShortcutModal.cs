using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Slack.NetStandard.Messages.Elements;
using Slack.NetStandard.Messages.TextEntities;
using Slack.NetStandard.Objects;
using Slack.NetStandard.WebApi.Chat;
using Slack.NetStandard.WebApi.Conversations;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/d_shortcutmodal")]
    [SlackAuth]
    public class D_ShortcutModal : ControllerBase
    {
        private readonly ILogger<InteractionController> _logger;
        private readonly ISlackApiClient _webapi;

        public D_ShortcutModal(ILogger<InteractionController> logger, ISlackApiClient webapi)
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
        public async Task<ActionResult> Post([FromForm] string payload)
        {
            return JsonConvert.DeserializeObject<InteractionPayload>(payload) switch
            {
                ViewSubmissionPayload submission => await SendApprovalRequest(submission),
                GlobalShortcutPayload shortcut => await GenerateModal(shortcut),
                BlockActionsPayload blocks => await SendResult(blocks),
                _ => new OkObjectResult("Unsupported - sorry!")
            };
        }

        private async Task<ActionResult> SendApprovalRequest(ViewSubmissionPayload submission)
        {
            var request = new PostMessageRequest
            {
                Channel = "C01C9M0DAMB",
                Blocks = GenerateApproval(submission)
            };
            await _webapi.Chat.Post(request);
            var channel = await GetDirectChannel(submission.User.ID);
            await _webapi.Chat.Post(new PostMessageRequest
            {
                Channel = channel,
                Text =
                    $"Your approval request for {submission.View.State.Values["request"].First().Value.Value} has been sent"
            });
            return new OkResult();
        }

        private List<IMessageBlock> GenerateApproval(ViewSubmissionPayload payload)
        {
            return new List<IMessageBlock>
            {
                new Header("Approval request"),
                new Section(new MarkdownText("*From*"), new MarkdownText(UserMention.Text(payload.User.ID))){BlockId = "originator"},
                new Section(new MarkdownText("*Request For*"), new PlainText(payload.View.State.Values["request"].First().Value.Value)){BlockId = "request"},
                new Actions
                {
                    Elements = new List<IMessageElement>
                    {
                        new Button{Text="Approve", Style = ButtonStyle.Primary, Value="approved"},
                        new Button{Text="Deny", Style = ButtonStyle.Danger, Value="declined"}
                    }
                }
            };
        }

        private async Task<ActionResult> GenerateModal(GlobalShortcutPayload shortcut)
        {
            if (shortcut.CallbackId != "approval_request")
            {
                return new OkResult();
            }

            var modal = new View
            {
                Type = "modal",
                Submit = "Send Request",
                Title = "Approval Request",
                Close = "Cancel",
                Blocks = new IMessageBlock[]
                {
                    new Input{Label="What would you like approval for?",BlockId = "request", Optional = false, Hint = "single line description of approval", Element = new PlainTextInput{ActionId = "text"}}, 
                    new Input{Label="Extra details to persuade someone?", BlockId="extra", Optional = true, Hint = "all details useful", Element = new PlainTextInput{ActionId="text"}}
                }
            };
            try
            {
                var result = await _webapi.View.Open(shortcut.TriggerId, modal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return new OkResult();
        }

        private async Task<ActionResult> SendResult(BlockActionsPayload blocks)
        {
            var originatorBlock = blocks.Message.Blocks.OfType<Section>().First(b => b.BlockId == "originator");
            var itemBlock = blocks.Message.Blocks.OfType<Section>().First(b => b.BlockId == "request");

            var originator = TextParser.FindEntities(originatorBlock.Fields.Last().Text).First() as UserMention;
            var item = itemBlock.Fields.Last().Text;
            var decision = blocks.Actions.First().Value;

            var approverMessage = $"Request {UserMention.Text(originator.UserId)} made for {item} was {decision} by {UserMention.Text(blocks.User.ID)}";
            var message = new Message { Text = approverMessage };
            await new HttpClient().PostAsync(blocks.ResponseUrl, new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json"));

            var channelId = await GetDirectChannel(originator.UserId);

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

        private async Task<string> GetDirectChannel(string userId)
        {
            var conversation = await _webapi.Conversations.Open(new ConversationOpenRequest
            {
                Users = userId
            });
            return conversation.Channel.ID;
        }
    }
}