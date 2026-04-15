using CCP.Shared.UIContext;
using CustomerService.Sdk.Services;
using IdentityService.Sdk.Services.Customer;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.InviteCustomer
{
    public partial class InviteCustomer : ComponentBase
    {
        private readonly ILogger<InviteCustomer> _logger;
        private readonly ICustomerService _customerService;
        private readonly ICustomerSdkService _customerSdkService;
        private readonly IUIUserContext _uIUserContext;
        private InviteCustomerModel InviteCustomerModel { get; set; } = new InviteCustomerModel();

        public InviteCustomer(ILogger<InviteCustomer> logger, ICustomerService customerService, ICustomerSdkService customerSdkService, IUIUserContext uIUserContext)
        {
            _logger = logger;
            _customerService = customerService;
            _customerSdkService = customerSdkService;
            _uIUserContext = uIUserContext;
        }

        private async Task Submit()
        {
            try
            {
                var result = await _customerService.InviteCustomer(InviteCustomerModel.Email);
                await _customerSdkService.CreateCustomer(new CustomerService.Sdk.Models.CreateCustomerRequest()
                {
                    Id = Guid.NewGuid(),
                    Email = InviteCustomerModel.Email,
                    Name = InviteCustomerModel.Email, // Assuming name is same as email for this example
                    OrganizationId = _uIUserContext.OrganizationId,
                });
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
