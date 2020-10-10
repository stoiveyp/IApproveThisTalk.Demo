using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Slack.NetStandard;

namespace IApproveThisTalk.Demo
{
    public class SlackAuthAttribute:Attribute,IAsyncAuthorizationFilter
    {
        private const string SignatureHeader = "X-Slack-Signature";
        private const string TimestampHeader = "X-Slack-Request-Timestamp";

        protected RequestVerifier Verifier { get; set; }

        protected virtual void EnsureVerifier()
        {
            Verifier ??= new RequestVerifier(Environment.GetEnvironmentVariable("signing_secret"));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context?.HttpContext?.Request;
            if (request?.Method != HttpMethods.Post)
            {
                return;
            }

            EnsureVerifier();

            if (!request.Headers.ContainsKey(SignatureHeader) || string.IsNullOrWhiteSpace(request.Headers[SignatureHeader]))
            {
                context.Result = new BadRequestObjectResult("No slack signature");
                return;
            }

            if (!request.Headers.ContainsKey(TimestampHeader) || !long.TryParse(request.Headers[TimestampHeader], out var timestamp))
            {
                context.Result = new BadRequestObjectResult("No slack timestamp");
                return;
            }

            request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            if (!Verifier.Verify(request.Headers[SignatureHeader], timestamp, body))
            {
                context.Result = new BadRequestObjectResult("Invalid slack signature");
            }
        }
    }
}
