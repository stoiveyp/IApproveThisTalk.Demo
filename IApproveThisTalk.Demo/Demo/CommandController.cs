﻿using Microsoft.AspNetCore.Mvc;

namespace IApproveThisTalk.Demo
{
    [ApiController]
    [Route("slack/temp")]
    [SlackAuth]
    public class CommandController:ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new OkObjectResult("Endpoint disabled");
        }

        [HttpPost]
        public ActionResult Post()
        {
            return new OkObjectResult("Unsupported - sorry!");
        }
    }
}
