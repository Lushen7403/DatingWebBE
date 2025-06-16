using System.ComponentModel.DataAnnotations;

namespace MatchLoveWeb.Models.DTO
{
    public class ChangePasswordDTO
    {
        [Required]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải từ 6 đến 100 ký tự")]
        public string NewPassword { get; set; }
    }
}
