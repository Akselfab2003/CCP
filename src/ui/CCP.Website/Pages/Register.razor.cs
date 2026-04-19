using CCP.Website.Components.Register;
using CCP.Website.Services;
using IdentityService.Sdk.Models;
using IdentityService.Sdk.Services.Tenant;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace CCP.Website.Pages
{
    public partial class Register : ComponentBase
    {
        private readonly ITenantService _tenantService;
        private readonly NavigationManager _navigationManager;
        private readonly IWebsiteReferencesService _websiteReferencesService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<Register> _logger;
        private int _currentStep = 0;
        private RegisterOrganizationModel? RegisterOrganizationModel { get; set; }
        private RegisterAccountModel? RegisterAccountModel { get; set; }
        public Register(ITenantService tenantService, ILogger<Register> logger, NavigationManager navigationManager, IWebsiteReferencesService websiteReferencesService, IDialogService dialogService)
        {
            _tenantService = tenantService;
            _logger = logger;
            _navigationManager = navigationManager;
            _websiteReferencesService = websiteReferencesService;
            _dialogService = dialogService;
        }

        private readonly string DoneIcon = "✓";
        public void stepChanged(int step)
        {
            _currentStep = step;
        }

        public bool IsStepCompleted(int Index)
        {
            if (Index < _currentStep)
            {
                return true;
            }
            else if (Index == _currentStep)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        private void RegisterOrganization(RegisterOrganizationModel model)
        {
            RegisterOrganizationModel = model;
        }
        private void RegisterAccount(RegisterAccountModel model)
        {
            RegisterAccountModel = model;
        }

        private async Task Submit()
        {
            try
            {
                CreateTenantDTO createTenant = new CreateTenantDTO
                {
                    OrganizationName = RegisterOrganizationModel!.CompanyName,
                    DomainName = RegisterOrganizationModel.Domain,
                    AdminUser = new CreateAdminUserDTO
                    {
                        FirstName = RegisterAccountModel!.FirstName,
                        LastName = RegisterAccountModel.LastName,
                        Email = RegisterAccountModel.Email,
                        Password = RegisterAccountModel.Password
                    }
                };
                var CreateNewTenantRequest = await _tenantService.CreateTenant(createTenant);

                if (CreateNewTenantRequest.IsSuccess)
                {
                    _logger.LogInformation("Tenant created successfully");
                    _navigationManager.NavigateTo(_websiteReferencesService.Login);
                }
                else
                {
                    _logger.LogError("Failed to create tenant: {Error}", CreateNewTenantRequest.Error);
                    await _dialogService.ShowErrorAsync("Error", $"Failed to create tenant: {CreateNewTenantRequest.Error.Description}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tenant");
            }
        }
    }
}
