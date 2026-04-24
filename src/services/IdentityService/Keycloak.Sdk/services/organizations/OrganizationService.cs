using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.ServiceDefaults;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Keycloak.Sdk.services.organizations
{
    internal class OrganizationService : IOrganizationKeycloakService
    {
        private readonly ILogger<OrganizationService> _logger;
        private readonly IKiotaApiClient<KeycloakClient> _apiClient;

        private KeycloakClient Client => _apiClient.Client;

        public OrganizationService(ILogger<OrganizationService> logger, IKiotaApiClient<KeycloakClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        private async Task<Result<Guid>> GetOrganizationIdByNameAsync(string organizationName, CancellationToken ct = default)
        {
            try
            {
                var organizations = await Client.Admin.Realms[Constants.REALM]
                                            .Organizations
                                            .GetAsync(req =>
                                            {
                                                req.QueryParameters.Search = organizationName;
                                                req.QueryParameters.Max = 1;
                                            }, cancellationToken: ct);

                if (organizations == null) return Result.Failure<Guid>(Error.Failure("GetOrganizationIdByNameAsync.nullResponse", "Received null response from Keycloak API while retrieving organization ID."));

                var organization = organizations.Where(org => org != null && !string.IsNullOrEmpty(org.Name))
                                                .SingleOrDefault(organizations => organizations.Name!.Equals(organizationName, StringComparison.CurrentCultureIgnoreCase));

                if (organization == null)
                {
                    _logger.LogWarning("Organization with name {OrganizationName} not found.", organizationName);
                    return Result.Failure<Guid>(Error.NotFound("GetOrganizationIdByNameAsync.notFound", $"Organization with name '{organizationName}' was not found."));
                }


                if (!Guid.TryParse(organization.Id, out Guid organizationId))
                {
                    _logger.LogError("Invalid organization ID format for organization with name {OrganizationName}. Received ID: {OrganizationId}", organizationName, organization.Id);
                    return Result.Failure<Guid>(Error.Failure("GetOrganizationIdByNameAsync.invalidIdFormat", $"The organization ID retrieved for '{organizationName}' is not in a valid GUID format."));
                }

                return Result.Success(organizationId);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<Guid>(Error.Validation("GetOrganizationIdByNameAsync.validationError", $"Validation error occurred while retrieving the organization ID: {ex.Message}")),
                    _ => Result.Failure<Guid>(Error.Failure("GetOrganizationIdByNameAsync.apiError", $"API error occurred while retrieving the organization ID: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization ID for organization with name {OrganizationName}", organizationName);
                return Result.Failure<Guid>(Error.Failure("GetOrganizationIdByNameAsync.failed", $"An error occurred while retrieving the organization ID: {ex.Message}"));
            }
        }


        public async Task<Result<Guid>> CreateOrganizationAsync(string organizationName, string DomainName, CancellationToken ct = default)
        {
            try
            {
                var organization = new OrganizationRepresentation()
                {
                    Name = organizationName,
                    Enabled = true,
                    Domains = new List<OrganizationDomainRepresentation>()
                    {
                        new OrganizationDomainRepresentation
                        {
                            Name = DomainName,
                            Verified = true
                        }
                    }
                };

                await Client.Admin.Realms[Constants.REALM]
                                  .Organizations
                                  .PostAsync(organization, cancellationToken: ct);

                var organizationIdResult = await GetOrganizationIdByNameAsync(organizationName, ct);

                return organizationIdResult;
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error creating organization with name: {OrganizationName}", organizationName);
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<Guid>(Error.Validation("CreateOrganizationAsync.validationError", $"Validation error occurred while creating the organization: {ex.Message}")),
                    409 => Result.Failure<Guid>(Error.Conflict("CreateOrganizationAsync.conflict", $"An organization with the same name already exists: {ex.Message}")),
                    _ => Result.Failure<Guid>(Error.Failure("CreateOrganizationAsync.apiError", $"API error occurred while creating the organization: {ex.Message}"))
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization with name: {OrganizationName}", organizationName);
                return Result.Failure<Guid>(Error.Failure("CreateOrganizationAsync.failed", $"An error occurred while creating the organization: {ex.Message}"));
            }
        }

        public async Task<Result> AddExistingMemberToOrganizationAsync(Guid OrgId, Guid userId, CancellationToken ct = default)
        {
            try
            {
                await Client.Admin.Realms[Constants.REALM].Organizations[OrgId.ToString()].Members.PostAsync(userId.ToString(), cancellationToken: ct);
                return Result.Success();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error adding user {UserId} to organization {OrganizationName}", userId, OrgId);
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation("AddMemberToOrganizationAsync.validationError", $"Validation error occurred while adding member to the organization: {ex.Message}")),
                    404 => Result.Failure(Error.NotFound("AddMemberToOrganizationAsync.notFound", $"The organization '{OrgId}' or user with ID '{userId}' was not found: {ex.Message}")),
                    _ => Result.Failure(Error.Failure("AddMemberToOrganizationAsync.apiError", $"API error occurred while adding member to the organization: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to organization {OrganizationName}", userId, OrgId);
                return Result.Failure(Error.Failure("AddMemberToOrganizationAsync.failed", $"An error occurred while adding member to the organization: {ex.Message}"));
            }
        }

        public async Task<Result> InviteMemberToOrg(Guid OrgId, string email, CancellationToken ct = default)
        {
            try
            {
                var invitation = new OrganizationInvitationRepresentation
                {
                    Email = email,
                    OrganizationId = OrgId.ToString()
                };
                await Client.Admin.Realms[Constants.REALM]
                                  .Organizations[OrgId.ToString()]
                                  .Members
                                  .InviteUser
                                  .PostAsync(new()
                                  {
                                      Email = email
                                  }, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error inviting user with email {Email} to organization {OrganizationName}", email, OrgId);
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation("InviteMemberToOrg.validationError", $"Validation error occurred while inviting member to the organization: {ex.Message}")),
                    404 => Result.Failure(Error.NotFound("InviteMemberToOrg.notFound", $"The organization '{OrgId}' was not found: {ex.Message}")),
                    _ => Result.Failure(Error.Failure("InviteMemberToOrg.apiError", $"API error occurred while inviting member to the organization: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting user with email {Email} to organization {OrganizationName}", email, OrgId);
                return Result.Failure(Error.Failure("InviteMemberToOrg.failed", $"An error occurred while inviting member to the organization: {ex.Message}"));
            }
        }
        public async Task<Result<string>> GetOrganizationNameByIdAsync(Guid orgId, CancellationToken ct = default)
        {
            try
            {
                var organization = await Client.Admin.Realms[Constants.REALM]
                                               .Organizations[orgId.ToString()]
                                               .GetAsync(cancellationToken: ct);

                if (organization is null)
                {
                    return Result.Failure<string>(
                        Error.NotFound("GetOrganizationNameByIdAsync.notFound", $"Organization with id '{orgId}' was not found."));
                }

                if (string.IsNullOrWhiteSpace(organization.Name))
                {
                    return Result.Failure<string>(
                        Error.Failure("GetOrganizationNameByIdAsync.emptyName", $"Organization '{orgId}' has no name."));
                }

                return Result.Success(organization.Name);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure<string>(
                        Error.NotFound("GetOrganizationNameByIdAsync.notFound", $"Organization with id '{orgId}' was not found.")),
                    400 => Result.Failure<string>(
                        Error.Validation("GetOrganizationNameByIdAsync.validationError", $"Invalid organization id '{orgId}'. {ex.Message}")),
                    _ => Result.Failure<string>(
                        Error.Failure("GetOrganizationNameByIdAsync.apiError", $"API error while retrieving organization name: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization name for org id {OrgId}", orgId);
                return Result.Failure<string>(
                    Error.Failure("GetOrganizationNameByIdAsync.failed", $"An error occurred while retrieving organization name: {ex.Message}"));
            }
        }


        public async Task<Result<KeycloakOrgDetails>> GetOrgDetails(Guid? OrgId = null, string? Domain = null, CancellationToken ct = default)
        {
            try
            {
                OrganizationRepresentation? organization = null;

                if (OrgId != null)
                {
                    organization = await Client.Admin.Realms[Constants.REALM]
                                                .Organizations[OrgId.ToString()]
                                                .GetAsync(cancellationToken: ct);
                }
                else if (!string.IsNullOrEmpty(Domain))
                {
                    var result = await Client.Admin.Realms[Constants.REALM].Organizations.GetAsync(req =>
                    {
                        req.QueryParameters.Search = Domain;
                    });

                    if (result != null)
                    {
                        organization = result.FirstOrDefault();
                    }
                    else
                    {
                        return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.nullResponse", "Received null response from Keycloak API while retrieving organization details."));
                    }
                }


                if (organization == null)
                {
                    return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.nullResponse", "Received null response from Keycloak API while retrieving organization details."));
                }

                var orgDetails = new KeycloakOrgDetails();

                if (organization.Id == null) return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.missingId", "The organization details retrieved from Keycloak API are missing the organization ID."));

                if (!Guid.TryParse(organization.Id, out Guid organizationId))
                {
                    _logger.LogError("Invalid organization ID format for organization with ID {OrganizationId}. Received ID: {OrganizationIdValue}", OrgId, organization.Id);
                    return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.invalidIdFormat", $"The organization ID retrieved for organization with ID '{OrgId}' is not in a valid GUID format."));
                }

                orgDetails.Id = organizationId;

                if (string.IsNullOrEmpty(organization.Name)) return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.missingName", "The organization details retrieved from Keycloak API are missing the organization name."));

                orgDetails.Name = organization.Name;

                if (organization.Domains == null || organization.Domains.Count == 0) return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.missingDomains", "The organization details retrieved from Keycloak API are missing organization domains."));

                orgDetails.DomainName = organization.Domains.FirstOrDefault()?.Name ?? string.Empty;

                return Result.Success(orgDetails);
            }
            catch (Exception ex)
            {
                return Result.Failure<KeycloakOrgDetails>(Error.Failure("GetOrgDetails.failed", $"An error occurred while retrieving organization details: {ex.Message}"));
            }
        }
    }
}
