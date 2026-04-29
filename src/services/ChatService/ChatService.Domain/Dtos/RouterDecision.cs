namespace ChatService.Domain.Dtos
{
    public class RouterDecision
    {
        public bool ShouldEscalate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}
