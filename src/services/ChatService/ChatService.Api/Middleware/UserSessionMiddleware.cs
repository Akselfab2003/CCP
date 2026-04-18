using ChatService.Application.AuthContext;
using ChatService.Application.Services.Domain;

namespace ChatService.Api.Middleware
{
    public class UserSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public UserSessionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IDomainServices domainServices, IActiveSession activeSession)
        {
            if (context == null) throw new ArgumentNullException("context");

            var orgin = context.Request.Headers.Origin.First();
            if (!context.Request.Cookies.TryGetValue("SessionId", out var SessionIdCookie)) await _next(context);
            if (orgin == null) await _next(context);

            if (SessionIdCookie != null)
            {
                activeSession.SetSessionId(Guid.Parse(SessionIdCookie));
            }

            var host = new Uri(orgin!).Host;
            var domainDetails = await domainServices.GetDomainDetails(host);
            if (domainDetails != null && domainDetails.IsSuccess)
            {
                activeSession.SetHost(host);
                activeSession.SetOrgId(domainDetails.Value.OrgId);
            }

            await _next(context);
        }
    }
}
