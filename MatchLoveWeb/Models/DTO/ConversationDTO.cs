namespace MatchLoveWeb.Models.DTO
{
    public class ConversationDto
    {
        public int Id { get; set; }

        public int User1Id { get; set; }

        public int User2Id { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

}
