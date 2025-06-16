using System.ComponentModel.DataAnnotations;

namespace MatchLoveWeb.Models.DTO
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
        public string Password { get; set; } = null!;
    }
}
