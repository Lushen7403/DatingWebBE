using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public int NotificationTypeId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ReferenceId { get; set; }

    public virtual NotificationType NotificationType { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
