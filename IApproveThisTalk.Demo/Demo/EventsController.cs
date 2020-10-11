using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.WebApi.Auth;

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
        public ActionResult Post(Event eventPayload)
        {
            return eventPayload switch
            {
                EventCallback callback => CallbackResponse(callback),
                UrlVerification verification => VerificationResponse(verification),
                _ => new OkResult()
            };
        }

        private ActionResult VerificationResponse(UrlVerification verification)
        {
            return new OkObjectResult(verification.Challenge);
        }

        private ActionResult CallbackResponse(EventCallback callback)
        {
            return callback.Event switch
            {
                _ => new OkResult()
            };
        }
    }
}
