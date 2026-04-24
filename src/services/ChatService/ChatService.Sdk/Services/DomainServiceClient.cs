using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.Logging;

namespace ChatService.Sdk.Services
{
    internal class DomainServiceClient : IDomainService
    {
        private readonly ILogger<DomainServiceClient> _logger;
        private readonly IKiotaApiClient<ChatServiceClient> _kiotaApiClient;
        private ChatServiceClient API => _kiotaApiClient.Client;

        public DomainServiceClient(ILogger<DomainServiceClient> logger, IKiotaApiClient<ChatServiceClient> kiotaApiClient)
        {
            _logger = logger;
            _kiotaApiClient = kiotaApiClient;
        }

        public async Task<Result> AddOrUpdateDomain(string domain)
        {
            try
            {
                await API.Configuration.AddOrUpdateDomain.PostAsync(q =>
                {
                    q.QueryParameters.Domain = domain;
                });

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating domain: {Domain}", domain);
                return Result.Failure(Error.Failure("DomainServiceError", "An error occurred while adding/updating the domain."));
            }
        }

        public async Task<Result<string>> GetDomain()
        {
            try
            {
                var response = await API.Configuration.GetDomain.GetAsync();
                if (response == null || string.IsNullOrEmpty(response))
                {
                    _logger.LogWarning("No domain found in response");
                    return Result.Failure<string>(Error.NotFound("DomainNotFound", "No domain found."));
                }
                return Result.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving domain");
                return Result.Failure<string>(Error.Failure("DomainServiceError", "An error occurred while retrieving the domain."));
            }
        }
    }
}
