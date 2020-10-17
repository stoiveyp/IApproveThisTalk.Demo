using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.WebApi.Chat;

namespace IApproveThisTalk.Demo.BreakGlass
{
    [ApiController]
    [Route("slack/b_sendmessage")]
    [SlackAuth]
    public class B_SendMessage: ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public async Task<ActionResult> Post()
        {
            SlashCommand slashCommand;
            using (var sr = new StreamReader(Request.Body))
            {
                slashCommand = new SlashCommand(sr.ReadToEnd());
            }

            var task = slashCommand.Command.Substring(1).ToLower() switch
            {
                "approve" => SendApprovalMessage(slashCommand),
                _ => Task.FromResult((ActionResult)new OkObjectResult("Unsupported command - sorry!"))
            };

            return await task;
        }

        private async Task<ActionResult> SendApprovalMessage(SlashCommand slashCommand)
        {
            var token = Environment.GetEnvironmentVariable("oauth_token");
            var api = new SlackWebApiClient(token);
            var request = new PostMessageRequest
            {
                Channel = "C01C9M0DAMB",
                Text = $"<@{slashCommand.UserId}> has requested approval for {slashCommand.Text}"
            };
            await api.Chat.Post(request);
            return new OkObjectResult($"Your approval request for {slashCommand.Text} has been sent");
        }
    }
}
