using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Slack.NetStandard.Interaction;

namespace IApproveThisTalk.Demo
{
    public static class SlackExtensions
    {
        public static SlashCommand ToSlashCommand(this IFormCollection form)
        {
            return new SlashCommand(form.ToDictionary(f => f.Key, f => f.Value.First()));
        }
    }
}
