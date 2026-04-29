namespace ChatService.Domain.Dtos
{
    public class AiGeneratedReply
    {
        public string Reply { get; set; } = string.Empty;
        public string? AlternativeReply { get; set; }
        public int Confidence { get; set; }
        public string Reasoning { get; set; } = string.Empty;
        public string? AgentHeadsUp { get; set; }
        public bool NeedsMoreInfo { get; set; }
        public string? InfoNeeded { get; set; }
    }
}
