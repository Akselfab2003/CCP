using System.Text;
using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using MailCow.Sdk.services.MailBox;
using Microsoft.Extensions.Logging;

namespace EmailService.Application.Services
{
    public class TenantEmailConfigurationService : ITenantEmailConfigurationService
    {
        private readonly ILogger<TenantEmailConfigurationService> _logger;
        private readonly IMailBoxManagementService _mailBoxManagementService;
        private readonly ITenantEmailConfigurationRepo _tenantEmailConfigurationRepo;
        private readonly ICurrentUser _currentUser;
        public TenantEmailConfigurationService(ILogger<TenantEmailConfigurationService> logger,
                                               ITenantEmailConfigurationRepo tenantEmailConfigurationRepo,
                                               ICurrentUser currentUser,
                                               IMailBoxManagementService mailBoxManagementService)
        {
            _logger = logger;
            _tenantEmailConfigurationRepo = tenantEmailConfigurationRepo;
            _currentUser = currentUser;
            _mailBoxManagementService = mailBoxManagementService;
        }


        public async Task<Result> AddTenantEmailConfigurationAsync(string DefaultSenderEmail)
        {
            try
            {
                var companyAlias = GenerateAlias(_currentUser.OrganizationName);
                var password = Guid.NewGuid().ToString(); // Generate a random password for the internal email

                var createMailboxResult = await _mailBoxManagementService.AddMailBox(companyAlias, "northflow.dev", password);

                if (createMailboxResult.IsFailure)
                {
                    _logger.LogError("Failed to create mailbox for tenant email configuration. Error: {Error}", createMailboxResult.Error.Description);
                    return Result.Failure(Error.Failure(code: "CreateMailboxError", description: "An error occurred while creating the mailbox for tenant email configuration."));
                }

                var tenantEmailConfiguration = new Domain.Models.TenantEmailConfiguration
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = _currentUser.OrganizationId,
                    DefaultSenderEmail = DefaultSenderEmail,
                    InternalEmail = $"{companyAlias}@northflow.dev",
                    InternalEmailPassword = password,
                };

                var addResult = await _tenantEmailConfigurationRepo.AddAsync(tenantEmailConfiguration);
                if (addResult.IsFailure)
                {
                    _logger.LogError("Failed to add tenant email configuration to the repository. Error: {Error}", addResult.Error.Description);
                    // Attempt to clean up the created mailbox if adding to the repository fails
                    //var deleteMailboxResult = await _mailBoxManagementService.DeleteMailBox(internalEmailAddress);
                    //if (deleteMailboxResult.IsFailure)
                    //{
                    //    _logger.LogError("Failed to clean up mailbox after repository failure. Mailbox: {InternalEmail}, Error: {Error}", internalEmailAddress, deleteMailboxResult.Error.Description);
                    //}
                    return Result.Failure(Error.Failure(code: "AddTenantEmailConfigurationError", description: "An error occurred while adding tenant email configuration."));

                }

                return addResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tenant email configuration for organization {OrganizationId}", _currentUser.OrganizationId);

                return Result.Failure(Error.Failure(code: "AddTenantEmailConfigurationError", description: "An error occurred while adding tenant email configuration."));
            }

        }


        public static string GenerateAlias(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return string.Empty;

            // Normalize: lowercase, trim, remove diacritics
            string alias = companyName.Trim().ToLowerInvariant();
            alias = System.Text.RegularExpressions.Regex.Replace(alias, @"\p{IsCombiningDiacriticalMarks}+", "");
            alias = alias.Normalize(NormalizationForm.FormD);

            // Replace spaces and invalid chars with dot
            alias = System.Text.RegularExpressions.Regex.Replace(alias, @"[^a-z0-9]", ".");
            // Collapse multiple dots
            alias = System.Text.RegularExpressions.Regex.Replace(alias, @"\.{2,}", ".");

            // Remove leading/trailing dots
            alias = alias.Trim('.');

            return alias;
        }
    }
}
