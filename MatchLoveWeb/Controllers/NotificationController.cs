using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchLoveWeb.Models;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly DatingWebContext _context; // Thay YourDbContext bằng tên DbContext của bạn

        public NotificationController(DatingWebContext context)
        {
            _context = context;
        }

        // GET: api/Notification/GetByAccountId/{accountId}
        [HttpGet("GetByAccountId/{accountId}")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotificationsByAccountId(int accountId)
        {
            try
            {
                var notifications = await _context.Notifications
                    .Where(n =>
                        n.UserId == accountId
                        || (n.NotificationTypeId == 2 && n.ReferenceId == null)
                    )
                    .OrderByDescending(n => n.CreatedAt) // Sắp xếp theo thời gian tạo mới nhất
                    .ToListAsync();

                if (notifications == null || !notifications.Any())
                {
                    return NotFound($"Không tìm thấy thông báo nào cho account ID: {accountId}");
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("MarkAllAsRead/{accountId}")]
        public async Task<IActionResult> MarkAllAsRead(int accountId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == accountId && n.IsRead == false)
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            var notificationsSys = await _context.Notifications
                .Where(n => n.NotificationTypeId == 2 && n.IsRead == false)
                .ToListAsync();

            foreach (var n in notificationsSys)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("UnreadCount/{accountId}")]
        public async Task<ActionResult<int>> GetUnreadCount(int accountId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == accountId && n.IsRead == false);

            var count2 = await _context.Notifications.CountAsync(p => p.NotificationTypeId == 2 && p.IsRead == false && (p.ReferenceId == null || p.UserId == accountId));
            return Ok(count + count2);
        }
    }
}