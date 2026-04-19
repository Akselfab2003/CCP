using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Application.Services.Faq
{
    public interface IFaqManagementService
    {
        Task<Result> CreateFaqAsync(string question, string answer);
        Task<Result> DeleteFaqAsync(int faqId);
        Task<Result<List<FaqEntity>>> GetAllFaqsAsync();
        Task<Result<List<FaqEntity>>> GetRelevantFaqAsync(string question);
        Task<Result<List<FaqEntity>>> SearchFaqAsync(string query);
        Task<Result> UpdateFaqAsync(int faqId, string question, string answer, string category);
    }
}
