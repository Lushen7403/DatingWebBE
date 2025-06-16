using AutoMapper;
using Azure.Core;
using Google.Apis.Auth;
using MatchLoveWeb.Models;
using MatchLoveWeb.Models.DTO;
using MatchLoveWeb.Services.Interfaces;
using MatchLoveWeb.SignaIR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly DatingWebContext _db;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly string _googleClientId;
        private readonly PasswordHasher<Account> _passwordHasher;

        public AccountController(
            IJwtTokenGenerator jwtTokenGenerator,
            DatingWebContext db,
            IMapper mapper,
            IHubContext<NotificationHub> notificationHub,
            IConfiguration config)    
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _db = db;
            _mapper = mapper;
            _notificationHub = notificationHub;
            _googleClientId = config["Authentication:Google:ClientId"];
            _passwordHasher = new PasswordHasher<Account>();
        }
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestDTO requestDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { Errors = errors });
                }
                var account = _db.Accounts.FirstOrDefault(a => a.UserName == requestDTO.UserName);
                if (account == null)
                    return Unauthorized("Sai tài khoản hoặc mật khẩu !!!");

                // Verify password hash
                var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.Password, requestDTO.Password);
                if (verifyResult == PasswordVerificationResult.Failed)
                    return Unauthorized("Sai tài khoản hoặc mật khẩu !!!");

                if (account.IsBanned == true)
                {
                    return Forbid("Tài khoản của bạn đã bị khóa, vui lòng liên hệ quản trị viên !!!");
                }

                // Nếu đúng thì generate token
                var token = _jwtTokenGenerator.GenerateToken(account.Id, account.RoleId.ToString());

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // Log lỗi (có thể sử dụng ILogger hoặc dịch vụ logging của bạn)
                return StatusCode(500, $"Có lỗi xảy ra khi đăng nhập !!!");
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO newAccountDTO)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { Errors = errors });
                }
                var existingUser = _db.Accounts.FirstOrDefault(a => a.UserName == newAccountDTO.UserName);
                if (existingUser != null)
                {
                    return BadRequest("Tài khoản đã tồn tại !!!");
                }

                var newAccount = _mapper.Map<Account>(newAccountDTO);
                newAccount.DiamondCount = 100;
                newAccount.CreatedAt = DateTime.Now;
                newAccount.IsBanned = false;
                newAccount.RoleId = 2;

                newAccount.Password = _passwordHasher.HashPassword(newAccount, newAccountDTO.Password);

                _db.Accounts.Add(newAccount);
                await _db.SaveChangesAsync();

                var welcomeNotif = new Notification
                {
                    UserId = newAccount.Id,
                    NotificationTypeId = 2,
                    Content = "Chào mừng bạn đến với MatchLove. Hãy kết bạn và làm quen với mọi người nhanh nào!",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ReferenceId = null
                };
                _db.Notifications.Add(welcomeNotif);
                await _db.SaveChangesAsync();

                await _notificationHub.Clients
                    .Group($"notif:{newAccount.Id}")
                    .SendAsync("ReceiveNotification", new
                    {
                        welcomeNotif.NotificationId,
                        welcomeNotif.Content,
                        welcomeNotif.CreatedAt,
                        welcomeNotif.NotificationTypeId,
                        welcomeNotif.ReferenceId
                    });

                return Ok("Đăng ký thành công");
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                return StatusCode(500, $"Có lỗi xảy ra khi đăng kí tài khoản !!! {ex.Message}");
            }

        }
        [HttpPost("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                return Ok("Đăng xuất thành công. Vui lòng xóa token phía client.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi xảy ra khi đăng xuất !!!");
            }
        }

        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { Errors = errors });
                }

                var account = _db.Accounts.FirstOrDefault(a => a.Id == request.AccountId);
                if (account == null)
                {
                    return NotFound("Tài khoản không tồn tại");
                }

                // Verify mật khẩu cũ
                var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.Password, request.OldPassword);
                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    return BadRequest("Mật khẩu cũ không đúng");
                }

                // Hash mật khẩu mới trước khi lưu
                account.Password = _passwordHasher.HashPassword(account, request.NewPassword);
                account.UpdatedAt = DateTime.Now;

                _db.Accounts.Update(account);
                _db.SaveChanges();

                return Ok("Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Có lỗi xảy ra khi đổi mật khẩu !!!");
            }
        }

        private async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleToken(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch
            {
                return null;
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
                return BadRequest("IdToken không được để trống");
            try
            {
                var payload = await VerifyGoogleToken(request.IdToken);

                if (payload == null)
                {
                    return Unauthorized("Token Google không hợp lệ");
                }

                // Lấy email từ token google
                var email = payload.Email;

                // Tìm tài khoản trong DB theo email
                var account = _db.Accounts.FirstOrDefault(a => a.Email == email);

                if (account == null)
                {
                    // Tạo mới tài khoản nếu chưa có
                    account = new Account
                    {
                        Email = email,
                        UserName = email, // Hoặc payload.Name nếu bạn thích
                        RoleId = 2, // user role mặc định
                        CreatedAt = DateTime.Now,
                        Password = "*******",
                        IsBanned = false,
                        DiamondCount = 100
                    };
                    _db.Accounts.Add(account);
                    await _db.SaveChangesAsync();
                }

                if (account.IsBanned == true)
                {
                    return Forbid("Tài khoản bị khóa");
                }

                var token = _jwtTokenGenerator.GenerateToken(account.Id, account.RoleId.ToString());

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                return StatusCode(500, "Lỗi khi đăng nhập bằng Google");
            }
        }



    }
}
