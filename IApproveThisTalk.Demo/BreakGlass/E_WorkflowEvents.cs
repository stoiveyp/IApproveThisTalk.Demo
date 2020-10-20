using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/E_WorkflowEvents")]
    [SlackAuth]
    public class E_WorkflowEvents
    {
        private ILogger<InteractionController> _logger;
        private readonly ISlackApiClient _webapi;

        public E_WorkflowEvents(ILogger<InteractionController> logger, ISlackApiClient client)
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
        public async Task<ActionResult> Post(Event eventPayload)
        {
            return eventPayload switch
            {
                EventCallback callback => await CallbackResponse(callback),
                UrlVerification verification => VerificationResponse(verification),
                _ => new OkResult()
            };
        }

        private ActionResult VerificationResponse(UrlVerification verification)
        {
            return new OkObjectResult(verification.Challenge);
        }

        private async Task<ActionResult> CallbackResponse(EventCallback callback)
        {
            return callback.Event switch
            {
                WorkflowStepExecute execute => await WorkflowExecute(execute),
                _ => new OkResult()
            };
        }

        private async Task<ActionResult> WorkflowExecute(WorkflowStepExecute execute)
        {
            if (execute.CallbackId != "approval_author")
            {
                return new BadRequestResult();
            }

            await _webapi.Workflow.StepCompleted(execute.WorkflowStep.WorkflowStepExecuteId,
                new Dictionary<string, object>
                {
                    {"approver", execute.WorkflowStep.Inputs["requestor"].Value}
                });
            return new OkResult();
        }
    }
}
