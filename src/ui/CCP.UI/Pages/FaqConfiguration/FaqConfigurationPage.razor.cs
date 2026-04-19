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
        private bool IsCreatingNewEntry = false;
        private FaqModel? NewFaqEntry { get; set; }

        private void StartCreateFaq()
        {
            NewFaqEntry = new FaqModel();
            IsCreatingNewEntry = true;
            ExpandedFaqId = 0;
        }
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
            try
            {
                var result = await _faqService.DeleteFaq(faqId);
                if (result.IsSuccess)
                    await ClearAndReload();
            }
            catch (Exception)
            {
            }


        }
        private async Task SaveNewFaqEntry()
        {
            if (NewFaqEntry is not null)
            {
                var result = await _faqService.CreateNewFaqEntry(NewFaqEntry.Question, NewFaqEntry.Answer);
                if (result.IsSuccess)
                {
                    await ClearAndReload();
                }
            }
        }

        private async Task SaveFaqEntry(FaqModel faqEntry)
        {
            try
            {
                var update = await _faqService.UpdateFaq(faqEntry.Id, faqEntry.Question, faqEntry.Answer, faqEntry.Category!);
                if (update.IsSuccess)
                {
                    await ClearAndReload();
                }
            }
            catch (Exception)
            {


            }
        }

        private async Task CancelCreateFaq()
        {
            ExpandedFaqId = 0;
            IsCreatingNewEntry = false;
            NewFaqEntry = null;
        }
        private async Task CancelEdit()
        {
            await ClearAndReload();
            ExpandedFaqId = 0;
        }

        private async Task ClearAndReload()
        {
            // Reload the list
            _faqEntries.Clear();
            var reload = await _faqService.GetAllFaqEntries();
            if (reload.IsSuccess)
                _faqEntries.AddRange(reload.Value);
            IsCreatingNewEntry = false;
            NewFaqEntry = null;

            StateHasChanged();
        }
    }
}
