using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Block
{
    public int Id { get; set; }

    public int BlockerId { get; set; }

    public int BlockedUserId { get; set; }

    public DateTime? BlockAt { get; set; }

    public virtual Account BlockedUser { get; set; } = null!;

    public virtual Account Blocker { get; set; } = null!;
}
