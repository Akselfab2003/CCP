using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Services.Faq
{
    public interface IFaqManagementService
    {
        Task<Result> CreateFaqAsync(string question, string answer);
    }
}