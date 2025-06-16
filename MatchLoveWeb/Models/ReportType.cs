using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class ReportType
{
    public int Id { get; set; }

    public string ReportTypeName { get; set; } = null!;

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
