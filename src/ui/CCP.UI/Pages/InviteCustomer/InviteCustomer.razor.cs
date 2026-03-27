using IdentityService.Sdk.Services.Customer;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.InviteCustomer
{
    public partial class InviteCustomer : ComponentBase
    {
        private readonly ILogger<InviteCustomer> _logger;
        private readonly ICustomerService _customerService;
        private InviteCustomerModel InviteCustomerModel { get; set; } = new InviteCustomerModel();

        public InviteCustomer(ILogger<InviteCustomer> logger, ICustomerService customerService)
        {
            _logger = logger;
            _customerService = customerService;
        }

        private async Task Submit()
        {
            try
            {
                var result = await _customerService.InviteCustomer(InviteCustomerModel.Email);
                if (result.IsFailure)
                {
                    _logger.LogError("Failed to invite customer with email {Email}: {Error}", InviteCustomerModel.Email, result.Error);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting customer with email {Email}", InviteCustomerModel.Email);
            }
        }
    }

    public class InviteCustomerModel
    {
        public string Email { get; set; } = string.Empty;
    }
}
