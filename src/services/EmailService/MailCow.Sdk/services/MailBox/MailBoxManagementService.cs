using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace MailCow.Sdk.services.MailBox
{
    internal class MailBoxManagementService : IMailBoxManagementService
    {
        private readonly ILogger<MailBoxManagementService> _logger;
        private readonly IKiotaApiClient<MailCowClient> _apiClient;

        public MailBoxManagementService(ILogger<MailBoxManagementService> logger, IKiotaApiClient<MailCowClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result> AddMailBox(string mailBoxName, string Domain, string Password, CancellationToken ct = default)
        {
            try
            {
                var response = await _apiClient.Client.Api.V1.Add.Mailbox.PostAsMailboxPostResponseAsync(new Api.V1.Add.Mailbox.MailboxPostRequestBody()
                {
                    Name = mailBoxName,
                    LocalPart = mailBoxName,
                    Domain = Domain,
                    Password = Password,
                    Password2 = Password,
                });

                _logger.LogInformation("Mailbox {MailBoxName} added successfully to domain {Domain}", mailBoxName, Domain);
                if (response == null) return Result.Failure(Error.Failure(code: "NullResponse", description: "Received null response from MailCow API"));

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
                _logger.LogError(ex, "An unexpected error occurred while adding the mailbox");
                return Result.Failure(Error.Failure(code: "Exception", description: "An unexpected error occurred while adding the mailbox"));
            }
        }

        //public async Task<Result> DeleteMailBox(string mailBoxName, CancellationToken ct = default)
        //{
        //    try
        //    {
        //        var deleteMailBoxResponse = await _apiClient.Client.Api.V1.DeletePath.Mailbox.PostAsMailboxPostResponseAsync(new Api.V1.Delete.Mailbox.MailboxPostRequestBody()
        //        {
        //            Items = new Api.V1.Delete.Mailbox.MailboxPostRequestBody_items()
        //            {
        //                AdditionalData = new Dictionary<string, object>()
        //                {
        //                    { "", new List<string> { mailBoxName } }
        //                }
        //            }
        //        });
        //    }
        //    catch (ApiException apiEx)
        //    {
        //        return apiEx.ResponseStatusCode switch
        //        {
        //            401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "Unauthorized access to MailCow API")),
        //            403 => Result.Failure(Error.Failure(code: "Forbidden", description: "Forbidden access to MailCow API")),
        //            _ => Result.Failure(Error.Failure(code: "ApiError", description: $"API error occurred with status code {apiEx.ResponseStatusCode}"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An unexpected error occurred while deleting the mailbox");
        //        return Result.Failure(Error.Failure(code: "Exception", description: "An unexpected error occurred while deleting the mailbox"));
        //    }
        //}
    }
}
