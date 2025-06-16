using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class MatchConditon
{
    public int Id { get; set; }

    public int? AccountId { get; set; }

    public int? MinAge { get; set; }

    public int? MaxAge { get; set; }

    public double? MaxDistanceKm { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? GenderId { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Gender? Gender { get; set; } = null!;
}
