using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Conversation
{
    public int Id { get; set; }

    public int User1Id { get; set; }

    public int User2Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Account User1 { get; set; } = null!;

    public virtual Account User2 { get; set; } = null!;
}
