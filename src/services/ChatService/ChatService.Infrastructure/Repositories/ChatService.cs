using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ChatService.Interfaces;
using ChatService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatService.Repositories
{
    public class ChatService : IChatService
    {
        private readonly IEmbeddingService _embeddings;
        private readonly IFaqRepository _faqRepo;
        private readonly ITicketClient _ticketClient;
        private readonly HttpClient _http;
        private readonly ChatOptions _opts;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IEmbeddingService embeddings,
            IFaqRepository faqRepo,
            ITicketClient ticketClient,
            HttpClient http,
            IOptions<ChatOptions> opts,
            ILogger<ChatService> logger)
        {
            _embeddings = embeddings;
            _faqRepo = faqRepo;
            _ticketClient = ticketClient;
            _http = http;
            _opts = opts.Value;
            _logger = logger;
        }

        public async Task<ChatResponse> HandleAsync(
            ChatRequest request, string bearerToken, CancellationToken ct = default)
        {
            // 1. Embed brugerens besked
            var queryEmbedding = await _embeddings.GetEmbeddingAsync(request.Message, ct);

            // 2. Søg i FAQ via cosine similarity
            var faqHits = await _faqRepo.SearchSimilarAsync(
                queryEmbedding, request.OrganizationId,
                _opts.TopKResults, _opts.SimilarityThreshold, ct);

            // 3. Byg system prompt med bruger-context og FAQ resultater
            var systemPrompt = BuildSystemPrompt(request, faqHits);

            // 4. Kald Ollama
            var ollamaReply = await CallOllamaAsync(systemPrompt, request, ct);

            // 5. Detect intent
            var intent = DetectIntent(request.Message, ollamaReply);

            int? ticketId = null;
            if (intent == "ticket")
            {
                var title = request.Message.Length > 100
                    ? request.Message[..100]
                    : request.Message;

                var note = $"Oprettet via chatbot af {request.UserName} ({request.UserEmail})\n\n{request.Message}";

                ticketId = await _ticketClient.CreateTicketAsync(
                    title, request.CustomerId, note, bearerToken, ct);
            }

            return new ChatResponse
            {
                Reply = ollamaReply,
                TicketCreated = ticketId.HasValue,
                TicketId = ticketId,
                Intent = faqHits.Any() ? "faq" : intent
            };
        }

        private string BuildSystemPrompt(ChatRequest request, List<FaqEntry> faqHits)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Du er en hjælpsom support-assistent.");
            sb.AppendLine($"Du taler med {request.UserName} ({request.UserEmail}).");
            sb.AppendLine("Svar altid på dansk, præcist og venligt.");
            sb.AppendLine();

            if (faqHits.Any())
            {
                sb.AppendLine("## Relevant vidensbase");
                sb.AppendLine("Brug nedenstående til at besvare spørgsmålet:");
                sb.AppendLine();
                foreach (var faq in faqHits)
                {
                    sb.AppendLine($"Spørgsmål: {faq.Question}");
                    sb.AppendLine($"Svar: {faq.Answer}");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("## Regler");
            sb.AppendLine("- Hvis du ikke kender svaret, sig det ærligt.");
            sb.AppendLine("- Hvis brugeren vil oprette en ticket, bekræft det og afslut svaret med: [OPRET_TICKET]");
            sb.AppendLine("- Opret kun ticket hvis brugeren eksplicit beder om det eller har et problem der kræver menneskelig hjælp.");

            return sb.ToString();
        }

        private async Task<string> CallOllamaAsync(
            string systemPrompt, ChatRequest request, CancellationToken ct)
        {
            var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

            foreach (var turn in request.History.TakeLast(10))
                messages.Add(new { role = turn.Role, content = turn.Content });

            messages.Add(new { role = "user", content = request.Message });

            var payload = new
            {
                model = _opts.ChatModel,
                messages,
                stream = false
            };

            var response = await _http.PostAsJsonAsync("/api/chat", payload, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

            return json
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Jeg kunne desværre ikke generere et svar.";
        }

        private static string DetectIntent(string userMessage, string botReply)
        {
            if (botReply.Contains("[OPRET_TICKET]", StringComparison.OrdinalIgnoreCase))
                return "ticket";

            var ticketKeywords = new[]
            {
            "opret ticket", "lav en sag",
            "support sag", "hjælp fra en person"
        };

            if (ticketKeywords.Any(k =>
                    userMessage.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "ticket";

            return "unknown";
        }
    }
}
