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
        private int ExpandedFaqId = 0;
        //private bool IsCreatingNewEntry = false;

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

        private void ToggleFaqEntry(int faqId)
        {
            if (ExpandedFaqId == faqId)
            {
                ExpandedFaqId = 0; // Collapse if the same entry is clicked
            }
            else
            {
                ExpandedFaqId = faqId; // Expand the clicked entry
            }
        }

        private async Task DeleteFaqEntry(int faqId)
        {


        }

        private async Task SaveFaqEntry(FaqModel faqEntry)
        {
        }

        private async Task CreateFaqEntry()
        {
        }

        private async Task CancelEdit()
        {
            ExpandedFaqId = 0;
        }
    }
}
