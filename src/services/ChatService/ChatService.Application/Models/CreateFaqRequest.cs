namespace ChatService.Application.Models
{
    public class CreateFaqRequest
    {
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }
}
