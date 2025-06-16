using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Match
{
    public int Id { get; set; }

    public int User1Id { get; set; }

    public int User2Id { get; set; }

    public DateTime? MatchedAt { get; set; }

    public virtual Account User1 { get; set; } = null!;

    public virtual Account User2 { get; set; } = null!;
}
