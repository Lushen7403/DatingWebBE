using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Report
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ReportedUserId { get; set; }

    public int ReportedTypeId { get; set; }

    public string Content { get; set; } = null!;

    public bool? IsChecked { get; set; }

    public DateTime? ReportAt { get; set; }

    public virtual ReportType ReportedType { get; set; } = null!;

    public virtual Account ReportedUser { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
