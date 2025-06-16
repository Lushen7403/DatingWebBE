namespace MatchLoveWeb.Models.DTO
{
    public class ConversationUserDTO
    {
        public int Id { get; set; }
        public int OtherUserId { get; set; }
        public string? OtherUserName { get; set; }
        public string? OtherUserAvatar { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
