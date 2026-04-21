using ChatService.Application.AuthContext;

namespace ChatService.Api.Middleware
{
    public class UserSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public UserSessionMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IAuthParser authParser)
        {
            if (context == null) throw new ArgumentNullException("context");

            var result = await authParser.ParseContext(context);
            await _next(context);
        }
    }
}
