using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class NotificationType
{
    public int Id { get; set; }

    public string NotificationTypeName { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
