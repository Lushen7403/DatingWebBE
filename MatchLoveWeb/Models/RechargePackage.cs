using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class RechargePackage
{
    public int Id { get; set; }

    public int Price { get; set; }

    public int DiamondCount { get; set; }

    public string? Description { get; set; }

    public bool IsActivate { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
