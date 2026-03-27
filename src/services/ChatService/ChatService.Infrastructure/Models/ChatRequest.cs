using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Models;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public Guid? CustomerId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<ConversationTurn> History { get; set; } = [];
}

public class ConversationTurn
{
    public string Role { get; set; } = string.Empty; // "user" | "assistant"
    public string Content { get; set; } = string.Empty;
}
