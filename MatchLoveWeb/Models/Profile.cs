using System;
using System.Collections.Generic;

namespace MatchLoveWeb.Models;

public partial class Profile
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public string? FullName { get; set; }

    public DateTime? Birthday { get; set; }

    public int GenderId { get; set; }

    public string? Description { get; set; }

    public string? Avatar { get; set; }
    public string? PublicId { get; set; }
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Gender Gender { get; set; } = null!;

    public virtual ICollection<ProfileImage> ProfileImages { get; set; } = new List<ProfileImage>();

    public ICollection<UserHobby> UserHobbies { get; set; }

}
