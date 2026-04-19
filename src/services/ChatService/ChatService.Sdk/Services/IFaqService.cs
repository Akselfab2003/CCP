using CCP.Shared.ResultAbstraction;
using ChatService.Sdk.Models;

namespace ChatService.Sdk.Services
{
    internal interface IFaqService
    {
        Task<Result> CreateNewFaqEntry(string question, string answer);
        Task<Result> DeleteFaq(int id);
        Task<Result<List<FaqModel>>> GetAllFaqEntries();
        Task<Result<List<FaqModel>>> SearchFaqEntries(string query);
        Task<Result> UpdateFaq(int id, string question, string answer, string category);
    }
}
