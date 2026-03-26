using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Models;

public class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public bool TicketCreated { get; set; }
    public int? TicketId { get; set; }
    public string Intent { get; set; } = string.Empty; // "faq" | "ticket" | "unknown"
}
