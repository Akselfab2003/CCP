namespace ChatService.Domain.Entities.AI
{
    public class GeneratedReply
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public int TicketId { get; set; }
        public string Reply { get; set; } = string.Empty;
        public string? AlternativeReply { get; set; }

        public int Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public string? AgentHeadsUp { get; set; }

        public bool NeedsMoreInfo { get; set; }
        public string? InfoNeeded { get; set; }

    }
}
