namespace TicketService.Application.Services.Ticket
{
    public class ManagerStatsDto
    {
        public int OpenTickets { get; set; }
        public int ClosedToday { get; set; }
        public string AvgResponseTime { get; set; } = "—";
        public int AwaitingUser { get; set; }
        public List<SupporterPerformanceDto> TeamPerformance { get; set; } = new();
    }
}
