using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Message
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    public int SenderId { get; set; }

    public string MessageText { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public string? ToxicLevel { get; set; }
    public int? ToxicTokenCount { get; set; }
    public double? ToxicDensity { get; set; }
    public bool? HasHeavyWord { get; set; }


    public virtual ICollection<MessageMedium> MessageMedia { get; set; } = new List<MessageMedium>();
}
