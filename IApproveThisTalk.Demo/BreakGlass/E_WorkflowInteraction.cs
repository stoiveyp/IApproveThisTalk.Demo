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
    [Route("slack/E_WorkflowInteraction")]
    [SlackAuth]
    public class E_WorkflowInteraction : ControllerBase
    {
        private readonly ILogger<InteractionController> _logger;
        private readonly ISlackApiClient _webapi;

        public E_WorkflowInteraction(ILogger<InteractionController> logger, ISlackApiClient client)
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
                WorkflowStepEditPayload workflow => await WorkflowApprover(workflow),
                ViewSubmissionPayload view => await ViewDecision(view),
                _ => new OkObjectResult("Unsupported - sorry!")
            };
        }

        private async Task<ActionResult> ViewDecision(ViewSubmissionPayload view)
        {
            if (view.View.Type == "workflow_step")
            {
                return await WorkflowView(view);
            }

            return new OkResult();
        }

        private async Task<ActionResult> WorkflowView(ViewSubmissionPayload view)
        {
            var requestor = view.View.State.Values.First().Value.Values.First().OtherFields["selected_user"].ToString();
            var workflow_step = new WorkflowStep
            {
                WorkflowStepEditId = view.WorkflowStep.WorkflowStepEditId,
                Inputs = new Dictionary<string, WorkflowInput>
                {
                    {"requestor", new WorkflowInput{Value = requestor}},
                    {"test", new WorkflowInput{Value="test"}}
                },
                Outputs = new[]
                {
                    new WorkflowOutput
                    {
                        Label = "Request Approver",
                        Name = "approver",
                        Type = WorkflowOutputType.User
                    }
                }
            };
            var result = await _webapi.Workflow.UpdateStep(workflow_step);
            return new OkResult();
        }

        private async Task<ActionResult> WorkflowApprover(WorkflowStepEditPayload workflow)
        {
            if (workflow.CallbackId != "approval_author")
            {
                return new OkResult();
            }

            var modal = new View
            {
                Type = "workflow_step",
                Blocks = new[]
                {
                    new Input {Label = "who is the requesting user?", Element = new UsersSelect{ActionId = "requestor",Placeholder = "pick the right user"}}
                }
            };

            var result = await _webapi.View.Open(workflow.TriggerId, modal);
            return new OkResult();
        }
    }
}
