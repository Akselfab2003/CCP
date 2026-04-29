using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.UserManagement
{
    public partial class UserManagement : ComponentBase
    {
        private string activeTab = "invite-supporter";

        private void SetActiveTab(string tab)
        {
            activeTab = tab;
        }
    }
}
