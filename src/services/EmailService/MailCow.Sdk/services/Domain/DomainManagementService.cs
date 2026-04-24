using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace MailCow.Sdk.services.Domain
{
    internal class DomainManagementService
    {
        private readonly ILogger<DomainManagementService> _logger;
        private readonly IKiotaApiClient<MailCowClient> _apiClient;

        public DomainManagementService(ILogger<DomainManagementService> logger, IKiotaApiClient<MailCowClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result> AddDomain(string domainName, CancellationToken ct = default)
        {
            try
            {

                var response = await _apiClient.Client.Api.V1.Add.Domain.PostAsync(new DomainPostRequestBody()
                {
                    Domain = domainName,
                }, cancellationToken: ct);

                if (response == null) return Result.Failure(Error.Failure(code: "NullResponse", description: "Received null response from MailCow API"));

                var allSuccess = response.All(res => res.Type == Domain_type.Success);

                if (!allSuccess)
                {
                    var errorMessages = response
                        .Where(m => m.Msg != null)
                        .Select(r =>
                        {
                            var value = r.Msg?.GetValue();
                            if (value is List<string> list)
                                return string.Join("; ", list);
                            if (value is string str)
                                return str;
                            return value?.ToString() ?? string.Empty;
                        });
                    return Result.Failure(Error.Failure(
                        code: "AddDomainFailed",
                        description: $"Failed to add domain {domainName}. API response: {string.Join(", ", errorMessages)}"
                    ));
                }

                return Result.Success();

            }
            catch (ApiException apiEx)
            {
                return apiEx.ResponseStatusCode switch
                {
                    401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "Unauthorized access to MailCow API")),
                    403 => Result.Failure(Error.Failure(code: "Forbidden", description: "Forbidden access to MailCow API")),
                    _ => Result.Failure(Error.Failure(code: "ApiError", description: $"API error occurred with status code {apiEx.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding domain {DomainName}", domainName);
                return Result.Failure(Error.Failure(code: "AddDomainError", description: $"An error occurred while adding the domain"));
            }
        }
    }
}
