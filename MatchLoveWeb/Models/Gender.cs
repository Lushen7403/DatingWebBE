using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Gender
{
    public int Id { get; set; }

    public string GenderName { get; set; } = null!;

    public virtual ICollection<MatchConditon> MatchConditons { get; set; } = new List<MatchConditon>();

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();
}
