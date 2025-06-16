namespace MatchLoveWeb.Models.DTO
{
    public class SwipeResponseDto
    {
        public int AccountId { get; set; }
        public int SwipedAccountId { get; set; }
        public bool SwipeAction { get; set; }
        public DateTime? SwipedAt { get; set; }
    }

}
