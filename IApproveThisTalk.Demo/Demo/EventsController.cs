﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;

namespace IApproveThisTalk.Demo.Demo
{
    [ApiController]
    [Route("slack/events")]
    [SlackAuth]
    public class EventsController
    {
        private ILogger<InteractionController> _logger;
        private readonly ISlackApiClient _webapi;

        public EventsController(ILogger<InteractionController> logger, ISlackApiClient client)
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
                UrlVerification verification => VerificationResponse(verification),
                _ => new OkResult()
            };
        }

        private ActionResult VerificationResponse(UrlVerification verification)
        {
            return new OkObjectResult(verification.Challenge);
        }
    }
}
