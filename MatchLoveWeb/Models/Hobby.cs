namespace MatchLoveWeb.Models
{
    public partial class Hobby
    {
        public int Id { get; set; }
        public string HobbyName { get; set; }

        // Navigation property
        public ICollection<UserHobby> UserHobbies { get; set; }
    }

}
