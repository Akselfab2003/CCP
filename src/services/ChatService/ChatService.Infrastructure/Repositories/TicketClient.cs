using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using ChatService.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Repositories
{
    public class TicketClient : ITicketClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<TicketClient> _logger;

        public TicketClient(HttpClient http, ILogger<TicketClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<int?> CreateTicketAsync(
            string title,
            Guid? customerId,
            string note,
            string bearerToken,
            CancellationToken ct = default)
        {
            var payload = new
            {
                title = title,
                customerId = customerId,
                assignedUserId = (Guid?)null,
                internalNotes = new[] { note }
            };

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

            try
            {
                var response = await _http.PostAsJsonAsync("/ticket/create", payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning(
                        "TicketService returnerede {Status}: {Error}",
                        response.StatusCode, error);
                    return null;
                }

                // /ticket/create returnerer Ok() uden body
                // Hent nyeste ticket for customer for at få id'et tilbage
                if (customerId.HasValue)
                {
                    var tickets = await _http.GetFromJsonAsync<List<TicketDto>>(
                        $"/ticket/GetTickets?customerId={customerId}", ct);

                    return tickets?
                        .OrderByDescending(t => t.CreatedAt)
                        .FirstOrDefault()?.Id;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved kald til TicketService.");
                return null;
            }
        }
    }

    file record TicketDto(int Id, DateTime CreatedAt);
}
