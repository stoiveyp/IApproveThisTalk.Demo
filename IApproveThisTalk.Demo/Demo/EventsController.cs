using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/events")]
    [SlackAuth]
    public class EventsController
    {
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
                return new OkResult();
            }

            return new OkResult();
        }
    }
}
