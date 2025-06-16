using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Swipe
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int SwipedAccountId { get; set; }

    public bool SwipeAction { get; set; }

    public DateTime? SwipedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Account SwipedAccount { get; set; } = null!;
}
