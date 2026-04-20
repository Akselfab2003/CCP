namespace TicketService.Domain.Entities
{
    public class TicketHistoryEntry
    {
        public int Id { get; private set; }
        public int TicketId { get; private set; }
        public Guid? ActorUserId { get; private set; }
        public string EventType { get; private set; } = string.Empty;
        public string? OldValue { get; private set; }
        public string? NewValue { get; private set; }
        public DateTimeOffset OccurredAt { get; private set; }

        private TicketHistoryEntry() { }

        public static TicketHistoryEntry Create(
            int ticketId,
            Guid? actorUserId,
            string eventType,
            string? oldValue,
            string? newValue)
        {
            return new TicketHistoryEntry
            {
                TicketId = ticketId,
                ActorUserId = actorUserId,
                EventType = eventType,
                OldValue = oldValue,
                NewValue = newValue,
                OccurredAt = DateTimeOffset.UtcNow
            };
        }
    }
}
