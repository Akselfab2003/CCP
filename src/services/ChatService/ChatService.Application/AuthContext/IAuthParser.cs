using CCP.Shared.ResultAbstraction;
using Microsoft.AspNetCore.Http;

namespace ChatService.Application.AuthContext
{
    public interface IAuthParser
    {
        Task<Result> ParseContext(HttpContext context);
    }
}