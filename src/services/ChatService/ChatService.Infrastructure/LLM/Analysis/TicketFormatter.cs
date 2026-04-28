
using ChatService.Domain.Dtos;

namespace ChatService.Infrastructure.LLM.Analysis
{
    public static class TicketFormatter
    {
        public static string FormatTicketForAnalysis(SupportTicket ticket)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Ticket ID: {ticket.TicketId}");
            sb.AppendLine($"Description: {ticket.Description}");
            sb.AppendLine();
            sb.AppendLine("--- Conversation ---");

            foreach (var msg in ticket.Messages.OrderBy(m => m.SentAt))
            {
                var role = msg.AuthorType switch
                {
                    MessageAuthorType.User => "User",
                    MessageAuthorType.Support => "Supporter",
                    _ => "Unknown"
                };

                sb.AppendLine($"[{role}] {msg.SentAt:HH:mm}: {msg.Content}");
            }

            sb.AppendLine("--- End Conversation ---");

            return sb.ToString();

        }

        public static string FormatThread(SupportTicket ticket)
        {
            if (ticket.Messages == null || ticket.Messages.Count == 0)
                return "No messages in this ticket.";

            var sb = new System.Text.StringBuilder();

            return string.Join("\n", ticket.Messages.OrderBy(m => m.SentAt).Select(m =>
            {
                var role = m.AuthorType switch
                {
                    MessageAuthorType.User => "USER",
                    MessageAuthorType.Support => "SUPPORT",
                    _ => "System"
                };
                return $"[{role}] {m.SentAt:HH:mm}: {m.Content}";
            }));
        }

        public static string FormatPastSolutions(List<SimilarTicket> tickets)
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                sb.AppendLine($"[Case #{i + 1}] - {ticket.SimilarityScore} match");
                sb.AppendLine($"Problem: {ticket.TicketAnalysis.ProblemSummary}");
                sb.AppendLine($"Category: {ticket.TicketAnalysis.Category}");
                sb.AppendLine($"Component: {ticket.TicketAnalysis.Component}");

                if (ticket.TicketAnalysis.Symptoms.Length > 0)
                    sb.AppendLine($"Symptoms: {string.Join(", ", ticket.TicketAnalysis.Symptoms)}");

                if (ticket.TicketAnalysis.ErrorCodes.Length > 0)
                    sb.AppendLine($"Error Codes: {string.Join(", ", ticket.TicketAnalysis.ErrorCodes)}");

                if (!string.IsNullOrEmpty(ticket.TicketAnalysis.RootCause))
                    sb.AppendLine($"Root Cause: {ticket.TicketAnalysis.RootCause}");

                if (ticket.TicketAnalysis.SolutionSteps?.Length > 0)
                {
                    sb.AppendLine("Steps:");
                    int stepIndex = 1;
                    foreach (var step in ticket.TicketAnalysis.SolutionSteps)
                    {
                        sb.AppendLine($"{stepIndex}. {step}");
                        stepIndex++;
                    }
                }

                if (ticket.TicketAnalysis.PreventionTips?.Length > 0)
                    sb.AppendLine($"Prevention: {string.Join(", ", ticket.TicketAnalysis.PreventionTips)}");

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
