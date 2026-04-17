using CCP.Shared.ResultAbstraction;

namespace ChatService.Sdk.Services
{
    internal interface IFaqService
    {
        Task<Result> CreateNewFaqEntry(string question, string answer);
    }
}