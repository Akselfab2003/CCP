using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using ChatService.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace ChatService.Sdk.Services
{
    internal class AIReplyClient : IAIReplyClient
    {
        private readonly ILogger<AIReplyClient> _logger;
        private readonly IKiotaApiClient<ChatServiceClient> _apiClient;

        public AIReplyClient(ILogger<AIReplyClient> logger, IKiotaApiClient<ChatServiceClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result<AiReply>> GetReply(int TicketId)
        {
            try
            {
                if (TicketId == 0) return Result.Failure<AiReply>(Error.Validation("InvalidTicketId", "TicketId must be greater than 0"));

                var result = await _apiClient.Client.AI.Generate.PostAsync(req =>
                {
                    req.QueryParameters.TicketId = TicketId;
                });

                if (result is null)
                {
                    _logger.LogWarning("No AI reply found for TicketId: {TicketId}", TicketId);
                    return Result.Failure<AiReply>(Error.NotFound("AIReplyNotFound", $"No AI reply found for TicketId: {TicketId}"));
                }

                AiReply aiReply = new AiReply
                {
                    Id = result.Id.HasValue ? result.Id.Value : Guid.Empty,
                    OrgId = result.OrgId.HasValue ? result.OrgId.Value : Guid.Empty,
                    TicketId = result.TicketId.HasValue ? result.TicketId.Value : 0,
                    Reply = result.Reply ?? string.Empty,
                    AlternativeReply = result.AlternativeReply ?? string.Empty,
                    Confidence = result.Confidence ?? 0,
                    Reasoning = result.Reasoning ?? string.Empty,
                    AgentHeadsUp = result.AgentHeadsUp ?? string.Empty,
                    NeedsMoreInfo = result.NeedsMoreInfo ?? false,
                    InfoNeeded = result.InfoNeeded
                };

                return aiReply is not null
                    ? Result.Success(aiReply)
                    : Result.Failure<AiReply>(Error.NotFound("AIReplyNotFound", $"No AI reply found for TicketId: {TicketId}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting AI reply for TicketId: {TicketId}", TicketId);
                return Result.Failure<AiReply>(Error.Failure("AIReplyClientError", $"An error occurred while getting AI reply for TicketId: {TicketId}"));
            }
        }
    }
}
