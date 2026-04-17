using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Application.Services.Faq
{
    public interface IFaqManagementService
    {
        Task<Result> CreateFaqAsync(string question, string answer);
        Task<Result<List<FaqEntity>>> GetRelevantFaqAsync(string question);
    }
}