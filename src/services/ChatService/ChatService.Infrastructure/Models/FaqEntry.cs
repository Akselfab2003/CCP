using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ChatService.Models;

public class FaqEntry
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid? OrgId { get; set; }
    public float[] Embedding { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
