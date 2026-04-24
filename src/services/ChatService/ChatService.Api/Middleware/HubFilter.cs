using ChatService.Application.AuthContext;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.Middleware
{
    public class HubFilter : IHubFilter
    {
        private readonly IAuthParser _authParser;

        public HubFilter(IAuthParser authParser) => _authParser = authParser;

        public async ValueTask<object?> InvokeMethodAsync(
            HubInvocationContext invocationContext,
            Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var httpContext = invocationContext.Context.GetHttpContext();
            await _authParser.ParseContext(httpContext!);
            var result = await next(invocationContext);
            return result;
        }
    }
}
