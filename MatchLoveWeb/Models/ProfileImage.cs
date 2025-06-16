using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class ProfileImage
{
    public int Id { get; set; }

    public int ProfileId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string ImageUrl { get; set; } = null!;
    public string? PublicId { get; set; }

    public virtual Profile Profile { get; set; } = null!;
}
