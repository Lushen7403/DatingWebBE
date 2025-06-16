namespace MatchLoveWeb.Models
{
    public partial class UserHobby
    {
        public int Id { get; set; }

        // Foreign keys
        public int HobbyId { get; set; }
        public int ProfileId { get; set; }

        // Navigation properties
        public Hobby Hobby { get; set; }
        public Profile Profile { get; set; }
    }

}
