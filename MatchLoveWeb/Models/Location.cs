using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Location
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
}
