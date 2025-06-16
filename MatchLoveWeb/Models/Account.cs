using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Account
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsBanned { get; set; }

    public int DiamondCount { get; set; }

    public virtual ICollection<Block> BlockBlockedUsers { get; set; } = new List<Block>();

    public virtual ICollection<Block> BlockBlockers { get; set; } = new List<Block>();

    public virtual ICollection<Conversation> ConversationUser1s { get; set; } = new List<Conversation>();

    public virtual ICollection<Conversation> ConversationUser2s { get; set; } = new List<Conversation>();

    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();

    public virtual ICollection<MatchConditon> MatchConditons { get; set; } = new List<MatchConditon>();

    public virtual ICollection<Match> MatchUser1s { get; set; } = new List<Match>();

    public virtual ICollection<Match> MatchUser2s { get; set; } = new List<Match>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Profile> Profiles { get; set; } = new List<Profile>();

    public virtual ICollection<Report> ReportReportedUsers { get; set; } = new List<Report>();

    public virtual ICollection<Report> ReportUsers { get; set; } = new List<Report>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Swipe> SwipeAccounts { get; set; } = new List<Swipe>();

    public virtual ICollection<Swipe> SwipeSwipedAccounts { get; set; } = new List<Swipe>();
}
