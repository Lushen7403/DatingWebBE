using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Voucher
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public byte DiscountPercent { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
