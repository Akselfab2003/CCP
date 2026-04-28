using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Analysis;
using ChatService.Infrastructure.LLM.Chat;
using ChatService.Infrastructure.LLM.Embedding;
using ChatService.Infrastructure.Persistence.Repositories;
using MessagingService.Sdk.Services;
using Microsoft.Extensions.Logging;
using TicketService.Sdk.Services.Ticket;

namespace ChatService.Application.Services.Automated
{
    public class AutomaticMessageGeneration : IAutomaticMessageGeneration
    {
        private readonly ILogger<AutomaticMessageGeneration> _logger;
        private readonly IMessageSdkService _messageSdkService;
        private readonly ITicketService _ticketService;
        private readonly ITicketAnalysisService _ticketAnalysisService;
        private readonly ICurrentUser _currentUser;
        private readonly ITicketEmbeddingOrchestrator _ticketEmbeddingOrchestrator;
        private readonly IGenerateReplyService _generateReplyService;
        private readonly ITicketEmbeddingRepository _ticketEmbeddingRepository;
        private readonly ITicketAnalysisRepository _ticketAnalysisRepository;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;
        public AutomaticMessageGeneration(ILogger<AutomaticMessageGeneration> logger,
                                          IMessageSdkService messageSdkService,
                                          ITicketService ticketService,
                                          ITicketAnalysisService ticketAnalysisService,
                                          ICurrentUser currentUser,
                                          ITicketEmbeddingOrchestrator ticketEmbeddingOrchestrator,
                                          IGenerateReplyService generateReplyService,
                                          ITicketEmbeddingRepository ticketEmbeddingRepository,
                                          ServiceAccountOverrider serviceAccountOverrider,
                                          ITicketAnalysisRepository ticketAnalysisRepository)
        {
            _logger = logger;
            _messageSdkService = messageSdkService;
            _ticketService = ticketService;
            _ticketAnalysisService = ticketAnalysisService;
            _currentUser = currentUser;
            _ticketEmbeddingOrchestrator = ticketEmbeddingOrchestrator;
            _generateReplyService = generateReplyService;
            _ticketEmbeddingRepository = ticketEmbeddingRepository;
            _serviceAccountOverrider = serviceAccountOverrider;
            _ticketAnalysisRepository = ticketAnalysisRepository;
        }

        private async Task<Result<SupportTicket>> GetTicket(int ticketId)
        {
            try
            {
                var TicketDetailsResult = await _ticketService.GetTicket(ticketId);
                if (TicketDetailsResult.IsFailure)
                    return Result.Failure<SupportTicket>(TicketDetailsResult.Error);

                var NewSupportTicketRequest = new SupportTicket()
                {
                    TicketId = TicketDetailsResult.Value.Id,
                    Title = TicketDetailsResult.Value.Title,
                    Description = TicketDetailsResult.Value.Description ?? string.Empty,
                    OrgId = TicketDetailsResult.Value.OrganizationId,
                };

                var messagesResult = await _messageSdkService.GetMessagesByTicketIdAsync(ticketId);

                if (messagesResult.IsFailure)
                    return Result.Failure<SupportTicket>(messagesResult.Error);

                var messages = messagesResult.Value;

                Guid customerId = TicketDetailsResult.Value.CustomerId.HasValue ? TicketDetailsResult.Value.CustomerId.Value : Guid.Empty;

                List<TicketMessage> messagesList = new List<TicketMessage>();

                foreach (var msg in messages.Items)
                {
                    MessageAuthorType authorType = msg.UserId.HasValue && msg.UserId.Value == customerId ? MessageAuthorType.User : MessageAuthorType.Support;
                    var sentAt = msg.UpdatedAtUtc.HasValue ? msg.UpdatedAtUtc.Value : msg.CreatedAtUtc;

                    if (!sentAt.HasValue)
                    {
                        _logger.LogWarning("Message with ID {MessageId} has no valid timestamp. Skipping.", msg.Id);
                        continue; // Skip messages without a valid timestamp
                    }

                    var ticketMessage = new TicketMessage
                    {
                        MessageId = msg.Id,
                        TicketId = msg.TicketId,
                        AuthorType = authorType,
                        Content = msg.Content,
                        SentAt = sentAt.Value.DateTime
                    };
                    messagesList.Add(ticketMessage);
                }

                NewSupportTicketRequest.Messages = messagesList;

                return Result.Success(NewSupportTicketRequest);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket details or messages for ticket ID {TicketId}", ticketId);
                return Result.Failure<SupportTicket>(Error.Failure("TicketRetrievalError", $"An error occurred while retrieving the ticket: {ex.Message}"));
            }
        }


        public async Task<Result> TicketCreatedAnalysis(int ticketId)
        {
            try
            {
                Result<SupportTicket> ticketResult = await GetTicket(ticketId);

                if (ticketResult.IsFailure)
                    return Result.Failure(ticketResult.Error);

                var ticket = ticketResult.Value;


                await _ticketEmbeddingOrchestrator.OnTicketCreatedAsync(ticket);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ticket created analysis for ticket ID {TicketId}", ticketId);
                return Result.Failure(Error.Failure("TicketCreatedAnalysisError", $"An error occurred during ticket analysis: {ex.Message}"));
            }

        }
        public async Task<Result> TicketClosedAnalysis(int ticketId)
        {
            try
            {
                Result<SupportTicket> ticketResult = await GetTicket(ticketId);

                if (ticketResult.IsFailure)
                    return Result.Failure(ticketResult.Error);

                var ticket = ticketResult.Value;


                await _ticketEmbeddingOrchestrator.OnTicketClosedAsync(ticket);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ticket closed analysis for ticket ID {TicketId}", ticketId);
                return Result.Failure(Error.Failure("TicketClosedAnalysisError", $"An error occurred during ticket analysis: {ex.Message}"));
            }

        }
        public async Task<Result> NewMessageAddedToTicketAnalysis(int ticketId)
        {
            try
            {
                Result<SupportTicket> ticketResult = await GetTicket(ticketId);

                if (ticketResult.IsFailure)
                    return Result.Failure(ticketResult.Error);

                var ticket = ticketResult.Value;


                await _ticketEmbeddingOrchestrator.OnNewMessageAsync(ticket);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during new message added to ticket analysis for ticket ID {TicketId}", ticketId);
                return Result.Failure(Error.Failure("NewMessageAddedAnalysisError", $"An error occurred during ticket analysis: {ex.Message}"));
            }
        }


        public async Task<Result<GeneratedReply>> GenerateReplyUsingAI(int ticketId)
        {
            try
            {
                _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);
                Result<SupportTicket> ticketResult = await GetTicket(ticketId);

                if (ticketResult.IsFailure)
                    return Result.Failure<GeneratedReply>(ticketResult.Error);

                var ticket = ticketResult.Value;

                var analysisResult = await _ticketAnalysisRepository.GetByTicketIdAsync(ticketId);

                if (analysisResult.IsFailure)
                    return Result.Failure<GeneratedReply>(analysisResult.Error);

                var analysis = analysisResult.Value;

                if (analysis.Embedding == null)
                    return Result.Failure<GeneratedReply>(Error.Failure("EmbeddingNotFound", "No embedding found for the ticket analysis."));

                var SimilaritySearchResult = await _ticketEmbeddingRepository.SemanticSearch(analysis.Embedding.ProblemVector, topK: 5);

                if (SimilaritySearchResult.IsFailure)
                    return Result.Failure<GeneratedReply>(Error.Failure("SemanticSearchError", "Failed to perform semantic search for relevant ticket information."));

                var aiReplyResult = await _generateReplyService.GenerateReply(ticket, analysis, SimilaritySearchResult.Value);

                if (aiReplyResult.IsFailure)
                    return Result.Failure<GeneratedReply>(aiReplyResult.Error);

                var aiReply = aiReplyResult.Value;

                return Result.Success(aiReply);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI reply for ticket ID {TicketId}", ticketId);
                return Result.Failure<GeneratedReply>(Error.Failure("AIReplyGenerationError", $"An error occurred while generating AI reply: {ex.Message}"));
            }
        }


        //public async Task<Result<string>> GenerateMessage(int ticketId)
        //{
        //    try
        //    {


        //        Result<TicketProblemAnalysis> analysisResult = await _ticketAnalysisService.ExtractProblemAsync(ticket: NewSupportTicketRequst);

        //        if (analysisResult.IsFailure)
        //            return Result.Failure<string>(Error.Failure("TicketAnalysisError", "Failed to analyze the ticket for problem extraction."));

        //        var problemAnalysis = analysisResult.Value;


        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error generating automatic message");
        //        return Result.Failure<string>(Error.Failure("AutomaticMessageGenerationError", "An error occurred while generating the automatic message."));
        //    }
        //}
    }
}
