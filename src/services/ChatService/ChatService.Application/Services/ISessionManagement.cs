using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Services
{
    public interface ISessionManagement
    {
        Task<Result> CreateSession(string Domain);
    }
}
