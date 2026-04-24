
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
                    MessageAuthorType.Supporter => "Supporter",
                    _ => "Unknown"
                };

                sb.AppendLine($"[{role}] {msg.SentAt:HH:mm}: {msg.Content}");
            }

            sb.AppendLine("--- End Conversation ---");

            return sb.ToString();

        }

    }
}
