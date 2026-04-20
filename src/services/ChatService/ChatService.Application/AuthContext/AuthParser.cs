using CCP.Shared.ResultAbstraction;
using ChatService.Application.Services.Domain;
using Microsoft.AspNetCore.Http;

namespace ChatService.Application.AuthContext
{
    public class AuthParser : IAuthParser
    {
        private readonly IActiveSession _activeSession;
        private readonly IDomainServices _domainServices;
        public AuthParser(IActiveSession activeSession, IDomainServices domainServices)
        {
            _activeSession = activeSession;
            _domainServices = domainServices;
        }

        public async Task<Result> ParseContext(HttpContext context)
        {
            try
            {
                var origin = context.Request.Headers.Origin.First();

                var cookie = context.Request.Cookies["SessionId"];

                if (!Guid.TryParse(cookie, out var sessionId)) return Result.Failure(Error.Failure("AuthContextParseError", "Invalid SessionId cookie format."));
                if (origin == null) return Result.Failure(Error.Failure("AuthContextParseError", "Origin header not found."));

                var host = new Uri(origin!).Host;
                var domainDetails = await _domainServices.GetDomainDetails(host);

                if (domainDetails != null && domainDetails.IsSuccess)
                {
                    _activeSession.SetHost(host);
                    _activeSession.SetSessionId(sessionId);
                    _activeSession.SetOrgId(domainDetails.Value.OrgId);
                }

                return Result.Success();
            }
            catch (Exception)
            {
                return Result.Failure(Error.Failure("AuthContextParseError", "An error occurred while parsing the authentication context."));
            }
        }
    }
}
