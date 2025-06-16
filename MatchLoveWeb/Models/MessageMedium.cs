using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class MessageMedium
{
    public int Id { get; set; }

    public int MessageId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Message Message { get; set; } = null!;

    public string PublicId { get; set; }
}
