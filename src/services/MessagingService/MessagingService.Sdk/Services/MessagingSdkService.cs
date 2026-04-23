using MessagingService.Sdk.Dtos;
using MessagingService.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using System.Net.Http;
using System.Net.Http.Json;

namespace MessagingService.Sdk.Services
{
    internal class MessagingSdkService : IMessageSdkService
    {
        private const string HttpClientName = "MessageService";

        private readonly IKiotaApiClient<MessagingServiceClient> _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MessagingSdkService> _logger;

        public MessagingSdkService(
            IKiotaApiClient<MessagingServiceClient> client,
            IHttpClientFactory httpClientFactory,
            ILogger<MessagingSdkService> logger)
        {
            _client = client;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<Result<MessageDto>> CreateMessageAsync(
            int ticketId,
            Guid organizationId,
            Guid? userId,
            string content,
            bool isInternalNote = false,
            string? attachmentUrl = null,
            string? attachmentFileName = null,
            string? attachmentContentType = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new CreateMessageRequest
                {
                    Content = content,
                    OrganizationId = organizationId,
                    UserId = userId,
                    TicketId = ticketId,
                    IsInternalNote = isInternalNote
                };

                request.AttachmentUrl = attachmentUrl;
                request.AttachmentFileName = attachmentFileName;
                request.AttachmentContentType = attachmentContentType;

                var response = await _client.Client.Api.Messages.PostAsync(
                    request,
                    cancellationToken: cancellationToken);

                return MapMessage(response);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<MessageDto>(Error.Validation(code: "BadRequest", description: "Invalid request parameters.")),
                    _ => Result.Failure<MessageDto>(Error.Failure(code: "MessagingServiceApiError", description: $"API error occurred with status code {ex.ResponseStatusCode}."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating message for ticketId {TicketId}", ticketId);
                return Result.Failure<MessageDto>(Error.Failure(code: "MessagingServiceError", description: "An error occurred while creating the message."));
            }
        }

        public async Task<Result<PagedMessagesDto>> GetMessagesByTicketIdAsync(
            int ticketId,
            int limit = 50,
            int? beforeMessageId = null,
            CancellationToken cancellationToken = default)
        {

            try
            {
                var response = await _client.Client.Api.Messages.Ticket[ticketId].GetAsync(config =>
                {
                    config.QueryParameters.Limit = limit;
                    config.QueryParameters.BeforeMessageId = beforeMessageId;
                }, cancellationToken);

                if (response is null)
                {
                    return new PagedMessagesDto();
                }

                return new PagedMessagesDto
                {
                    HasMore = response.HasMore ?? false,
                    Items = response.Items?.Select(MapMessage)
                                  .Where(x => x is not null)
                                  .Cast<MessageDto>()
                                  .ToList()
                              ?? new List<MessageDto>()
                };
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error occurred while fetching messages for ticketId {TicketId}. Status Code: {StatusCode}", ticketId, ex.ResponseStatusCode);
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<PagedMessagesDto>(Error.Validation(code: "BadRequest", description: "Invalid request parameters.")),
                    _ => Result.Failure<PagedMessagesDto>(Error.Failure(code: "MessagingServiceApiError", description: $"API error occurred with status code {ex.ResponseStatusCode}."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching messages for ticketId {TicketId}", ticketId);
                return Result.Failure<PagedMessagesDto>(Error.Failure(code: "MessagingServiceError", description: "An error occurred while fetching messages."));
            }
        }

        public async Task<Result<MessageDto>> GetMessageByIdAsync(
            int messageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.Client.Api.Messages[messageId].GetAsync(
                    cancellationToken: cancellationToken);

                return MapMessage(response);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error occurred while fetching message with id {MessageId}. Status Code: {StatusCode}", messageId, ex.ResponseStatusCode);
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure<MessageDto>(Error.NotFound(code: "MessageNotFound", description: $"Message with id {messageId} not found.")),
                    _ => Result.Failure<MessageDto>(Error.Failure(code: "MessagingServiceApiError", description: $"API error occurred with status code {ex.ResponseStatusCode}."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching message with id {MessageId}", messageId);
                return Result.Failure<MessageDto>(Error.Failure(code: "MessagingServiceError", description: "An error occurred while fetching the message."));
            }
        }

        public async Task<Result<MessageDto>> UpdateMessageAsync(
            int messageId,
            string content,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new UpdateMessageRequest
                {
                    Content = content
                };

                var response = await _client.Client.Api.Messages[messageId].PutAsync(
                    request,
                    cancellationToken: cancellationToken);

                return MapMessage(response);

            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<MessageDto>(Error.Validation(code: "BadRequest", description: "Invalid request parameters.")),
                    404 => Result.Failure<MessageDto>(Error.NotFound(code: "MessageNotFound", description: $"Message with id {messageId} not found.")),
                    _ => Result.Failure<MessageDto>(Error.Failure(code: "MessagingServiceApiError", description: $"API error occurred with status code {ex.ResponseStatusCode}."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating message with id {MessageId}", messageId);
                return Result.Failure<MessageDto>(Error.Failure(code: "MessagingServiceError", description: "An error occurred while updating the message."));
            }
        }

        public async Task<Result> DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.Client.Api.Messages[messageId].DeleteAsync(cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure(Error.NotFound(code: "MessageNotFound", description: $"Message with id {messageId} not found.")),
                    _ => Result.Failure(Error.Failure(code: "MessagingServiceApiError", description: $"API error occurred with status code {ex.ResponseStatusCode}."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting message with id {MessageId}", messageId);
                return Result.Failure(Error.Failure(code: "MessagingServiceError", description: "An error occurred while deleting the message."));
            }
        }

        public async Task<Result<AttachmentDto>> UploadAttachmentAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
                memoryStream.Position = 0;

                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(memoryStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(streamContent, "file", fileName);

                var httpClient = _httpClientFactory.CreateClient(HttpClientName);
                var response = await httpClient.PostAsync("api/attachments", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    return Result.Failure<AttachmentDto>(Error.Failure(
                        code: "AttachmentUploadFailed",
                        description: $"Upload failed with status {(int)response.StatusCode}."));
                }

                var dto = await response.Content.ReadFromJsonAsync<AttachmentDto>(
                    cancellationToken: cancellationToken);

                if (dto is null)
                    return Result.Failure<AttachmentDto>(Error.Failure(code: "AttachmentUploadFailed", description: "Empty response from server."));

                // Make the URL absolute so the browser can load it directly
                if (dto.Url is not null && !dto.Url.StartsWith("http"))
                {
                    var baseAddress = httpClient.BaseAddress?.ToString().TrimEnd('/') ?? string.Empty;
                    dto.Url = $"{baseAddress}{dto.Url}";
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment {FileName}", fileName);
                return Result.Failure<AttachmentDto>(Error.Failure(code: "AttachmentUploadError", description: "An error occurred while uploading the attachment."));
            }
        }

        private static MessageDto? MapMessage(MessageResponse? response)
        {
            if (response is null)
                return null;

            return new MessageDto
            {
                Id = response.Id ?? 0,
                TicketId = response.TicketId ?? 0,
                UserId = response.UserId,
                OrganizationId = response.OrganizationId ?? Guid.Empty,
                Content = response.Content ?? string.Empty,
                CreatedAtUtc = response.CreatedAtUtc,
                UpdatedAtUtc = response.UpdatedAtUtc,
                IsEdited = response.IsEdited ?? false,
                IsDeleted = response.IsDeleted ?? false,
                IsInternalNote = response.IsInternalNote ?? false,
                DeletedAtUtc = response.DeletedAtUtc,
                AttachmentUrl = response.AttachmentUrl,
                AttachmentFileName = response.AttachmentFileName,
                AttachmentContentType = response.AttachmentContentType
            };
        }

    }
}
