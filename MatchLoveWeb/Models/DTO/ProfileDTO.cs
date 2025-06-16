using System.ComponentModel.DataAnnotations;

namespace MatchLoveWeb.Models.DTO
{
    public class ProfileDTO
    {
        public int AccountId { get; set; }
        [StringLength(100)]
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public int? GenderId { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }

        // Avatar (1 file)
        public IFormFile? Avatar { get; set; }

        // Ảnh phụ (nhiều file)
        public List<IFormFile>? ProfileImages { get; set; }

        public List<int>? HobbyIds { get; set; }
    }
}
