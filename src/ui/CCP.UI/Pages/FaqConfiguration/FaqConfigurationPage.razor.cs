using ChatService.Sdk.Models;
using ChatService.Sdk.Services;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.FaqConfiguration
{
    public partial class FaqConfigurationPage : ComponentBase
    {
        private readonly IFaqService _faqService;

        public FaqConfigurationPage(IFaqService faqService)
        {
            _faqService = faqService;
        }

        private readonly List<FaqModel> _faqEntries = [];

        protected override async Task OnInitializedAsync()
        {
            var result = await _faqService.GetAllFaqEntries();
            if (result.IsSuccess)
            {
                _faqEntries.AddRange(result.Value);
            }
            else
            {

            }
        }
    }
}
