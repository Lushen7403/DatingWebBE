using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchLoveWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminNotificationController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminNotificationController(DatingWebContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách notification hệ thống (NotificationTypeId = 2).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSystemNotifications(
                        [FromQuery] int page = 1,
                        [FromQuery] int pageSize = 10)
        {
            try
            {
                // Tạo queryable với filter NotificationTypeId = 2
                var query = _context.Notifications
                    .Include(n => n.NotificationType)
                    .Include(n => n.User)
                    .Where(n => n.NotificationTypeId == 2 && n.UserId ==16)
                    .OrderByDescending(n => n.CreatedAt)
                    .AsQueryable();

                // Tính tổng số bản ghi và số trang
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Lấy danh sách phân trang
                var notifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Where(p => p.User.RoleId == 1)
                    .Select(n => new
                    {
                        n.NotificationId,
                        n.UserId,
                        UserName = n.User.UserName,
                        n.NotificationTypeId,
                        TypeName = n.NotificationType.NotificationTypeName,
                        n.Content,
                        n.IsRead,
                        n.ReferenceId,
                        n.CreatedAt
                    })
                    .ToListAsync();

                // Trả về object bao gồm data và thông tin phân trang
                return Ok(new
                {
                    notifications,
                    totalItems,
                    totalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }


        /// <summary>
        /// Tạo mới 1 notification hệ thống (NotificationTypeId = 2).
        /// Nhận vào body chỉ với UserId, Content và ReferenceId (nếu có).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSystemNotification([FromBody] SystemNotificationRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Content))
                return BadRequest("Vui lòng truyền đúng UserId và Content.");

            try
            {
                var notification = new Notification
                {
                    UserId = req.UserId,
                    NotificationTypeId = 2,                // ép kiểu system
                    Content = req.Content,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ReferenceId = null              // hoặc để nguyên null
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetSystemNotifications),
                    null,
                    new
                    {
                        notification.NotificationId,
                        notification.UserId,
                        notification.NotificationTypeId,
                        notification.Content,
                        notification.IsRead,
                        notification.ReferenceId,
                        notification.CreatedAt
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa notification theo NotificationId
        /// </summary>
        [HttpDelete("{notificationId}")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

                if (notification == null)
                    return NotFound($"Không tìm thấy notification với ID: {notificationId}");

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return NoContent(); // 204 - xóa thành công, không có nội dung trả về
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

    }

    /// <summary>
    /// Chỉ chứa hai trường cần thiết để tạo notification system
    /// </summary>
    public class SystemNotificationRequest
    {
        public int UserId { get; set; }
        public string Content { get; set; } = null!;
    }
}
