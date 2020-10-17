using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.Messages.Elements;
using Slack.NetStandard.Messages.TextEntities;
using Slack.NetStandard.WebApi.Chat;

namespace IApproveThisTalk.Demo.BreakGlass
{
    [ApiController]
    [Route("slack/commands")]
    [SlackAuth]
    public class C_BlockKit : ControllerBase
    {
        private readonly ISlackApiClient _webapi;

        public C_BlockKit(ISlackApiClient webapi)
        {
            _webapi = webapi;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            SlashCommand slashCommand;
            string text = string.Empty;
            using (var sr = new StreamReader(Request.Body))
            {
                text = await sr.ReadToEndAsync();
                slashCommand = new SlashCommand(text);
            }

            var task = slashCommand.Command.Substring(1).ToLower() switch
            {
                "approve" => SendApprovalMessage(slashCommand),
                _ => Task.FromResult((ActionResult)new OkObjectResult("Unsupported command - sorry!"))
            };

            return await task;
        }

        private List<IMessageBlock> GenerateApproval(SlashCommand command)
        {
            return new List<IMessageBlock>
            {
                new Header("Approval request"),
                new Section(new MarkdownText("*From*"), new MarkdownText(UserMention.Text(command.UserId))){BlockId = "originator"},
                new Section(new MarkdownText("*Request For*"), new PlainText(command.Text)){BlockId = "request"},
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

        private async Task<ActionResult> SendApprovalMessage(SlashCommand command)
        {
            var request = new PostMessageRequest
            {
                Channel = "C01C9M0DAMB",
                Blocks = GenerateApproval(command)
            };
            var response = await _webapi.Chat.Post(request);
            return new OkObjectResult($"Your approval request for {command.Text} has been sent");
        }
    }
}