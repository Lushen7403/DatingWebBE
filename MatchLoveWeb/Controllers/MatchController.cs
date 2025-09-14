using MatchLoveWeb.Models;
using MatchLoveWeb.Models.Dto;
using MatchLoveWeb.Models.DTO;
using MatchLoveWeb.SignaIR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly DatingWebContext _db;

        private readonly IHubContext<NotificationHub> _notificationHub;
        public MatchController(DatingWebContext db, IHubContext<NotificationHub> notificationHub)
        {
            _db = db;
            _notificationHub = notificationHub;
        }


        // POST: api/match/swipe
        [HttpPost("swipe")]
        public async Task<IActionResult> Swipe([FromBody] SwipeDto dto)
        {
            var SwipedAccountId = _db.Profiles.FirstOrDefault(p => p.Id == dto.SwipedProfileId).AccountId;
            var swiperProfile = await _db.Profiles
                        .FirstOrDefaultAsync(p => p.AccountId == dto.AccountId);

            if (dto.AccountId == SwipedAccountId)
                return BadRequest("Không thể quẹt chính mình.");

            try
            {
                var account = await _db.Accounts
                                .FirstOrDefaultAsync(a => a.Id == dto.AccountId);
                if (account == null)
                    return NotFound("Tài khoản không tồn tại.");

                // Xác định ngày hôm nay (UTC)
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                // Đếm số lượt swipe đã dùng trong ngày
                var todaySwipeCount = await _db.Swipes.CountAsync(s =>
                    s.AccountId == dto.AccountId &&
                    s.SwipedAt >= today &&
                    s.SwipedAt < tomorrow);

                // Nếu đã dùng ≥10 lượt, trừ Diamond
                if (todaySwipeCount >= 10)
                {
                    const int costPerSwipe = 10;
                    if (account.DiamondCount < costPerSwipe)
                        return BadRequest("Không đủ kim cương để quẹt thêm.");

                    account.DiamondCount -= costPerSwipe;
                    // Cập nhật luôn số kim cương
                    _db.Accounts.Update(account);
                }

                var now = DateTime.UtcNow;
                var swipeEntity = await _db.Swipes
                    .FirstOrDefaultAsync(s =>
                        s.AccountId == dto.AccountId &&
                        s.SwipedAccountId == SwipedAccountId);
                if (swipeEntity != null)
                {
                    swipeEntity.SwipeAction = dto.SwipeAction;
                    swipeEntity.SwipedAt = now;
                    _db.Swipes.Update(swipeEntity);
                }
                else
                {
                    swipeEntity = new Swipe
                    {
                        AccountId = dto.AccountId,
                        SwipedAccountId = SwipedAccountId,
                        SwipeAction = dto.SwipeAction,
                        SwipedAt = now
                    };
                    _db.Swipes.Add(swipeEntity);
                }

                await _db.SaveChangesAsync();



                // Nếu like thì kiểm tra match
                if (dto.SwipeAction)
                {
                    var notif = new Notification
                    {
                        UserId = SwipedAccountId,
                        NotificationTypeId = 4, //like
                        Content = $"Người dùng {swiperProfile.FullName} đã thích bạn!",
                        IsRead = false,
                        CreatedAt = now,
                        ReferenceId = dto.AccountId
                    };
                    _db.Notifications.Add(notif);
                    await _db.SaveChangesAsync();

                    // Push realtime cho người được like
                    await _notificationHub.Clients
                        .Group($"notif:{SwipedAccountId}")
                        .SendAsync("ReceiveNotification", new
                        {
                            notif.NotificationId,
                            notif.Content,
                            notif.CreatedAt,
                            notif.NotificationTypeId,
                            notif.ReferenceId
                        });

                    var reverse = await _db.Swipes.FirstOrDefaultAsync(s =>
                        s.AccountId == SwipedAccountId &&
                        s.SwipedAccountId == dto.AccountId &&
                        s.SwipeAction);

                    if (reverse != null)
                    {
                        // Nếu chưa có match, tạo mới
                        var already = await _db.Matches.AnyAsync(m =>
                            (m.User1Id == dto.AccountId && m.User2Id == SwipedAccountId) ||
                            (m.User1Id == SwipedAccountId && m.User2Id == dto.AccountId));

                        if (!already)
                        {

                            var match = new Match
                            {
                                User1Id = dto.AccountId,
                                User2Id = SwipedAccountId,
                                MatchedAt = DateTime.UtcNow
                            };
                            _db.Matches.Add(match);

                            var conversation = new Conversation
                            {
                                User1Id = dto.AccountId,
                                User2Id = SwipedAccountId,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _db.Conversations.Add(conversation);

                            await _db.SaveChangesAsync();

                            var notifA = new Notification
                            {
                                UserId = dto.AccountId,
                                NotificationTypeId = 3, // ví dụ 2 = “match”
                                Content = $"Bạn đã match với {account.UserName}!",
                                IsRead = false,
                                CreatedAt = now,
                                ReferenceId = conversation.Id
                            };
                            var notifB = new Notification
                            {
                                UserId = SwipedAccountId,
                                NotificationTypeId = 3, //match
                                Content = $"Bạn đã match với {account.UserName}!",
                                IsRead = false,
                                CreatedAt = now,
                                ReferenceId = conversation.Id
                            };
                            _db.Notifications.AddRange(notifA, notifB);
                            await _db.SaveChangesAsync();

                            // Push realtime cả 2 bên
                            var payload = new
                            {
                                notifA.NotificationId,
                                notifA.Content,
                                notifA.CreatedAt,
                                notifA.NotificationTypeId,
                                notifA.ReferenceId
                            };
                            await _notificationHub.Clients
                                .Group($"notif:{dto.AccountId}")
                                .SendAsync("ReceiveNotification", payload);
                            await _notificationHub.Clients
                                .Group($"notif:{SwipedAccountId}")
                                .SendAsync("ReceiveNotification", payload);

                            return Ok(new ConversationDto
                            {
                                Id = conversation.Id,
                                User1Id = conversation.User1Id,
                                User2Id = conversation.User2Id,
                                CreatedAt = conversation.CreatedAt,
                                UpdatedAt = conversation.UpdatedAt
                            });
                        }
                    }
                }

                // Nếu không match thì trả về chính object swipe
                return Ok(new SwipeResponseDto
                {
                    AccountId = swipeEntity.AccountId,
                    SwipedAccountId = swipeEntity.SwipedAccountId,
                    SwipeAction = swipeEntity.SwipeAction,
                    SwipedAt = swipeEntity.SwipedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("swipers/{accountId}")]
        public async Task<IActionResult> GetSwipers(int accountId)
        {
            try
            {
                var baseUrl = "https://res.cloudinary.com/dfvhhpkyg/image/upload/";

                // Lấy danh sách AccountId đã bị accountId block
                var blockedAccountIds = await _db.Blocks
                    .Where(b => b.BlockerId == accountId)
                    .Select(b => b.BlockedUserId)
                    .ToListAsync();

                var swipers = await _db.Swipes
                    .Where(s => s.SwipedAccountId == accountId && s.SwipeAction && !blockedAccountIds.Contains(s.AccountId))
                    .Join(_db.Profiles,
                        swipe => swipe.AccountId,
                        profile => profile.AccountId,
                        (swipe, profile) => new
                        {
                            AccountId = swipe.AccountId,
                            Avatar = profile.Avatar != null ? baseUrl + profile.Avatar : null,
                            profile.FullName,
                            IsMatched = _db.Swipes.Any(match =>
                                match.AccountId == swipe.SwipedAccountId &&
                                match.SwipedAccountId == accountId &&
                                match.SwipeAction)
                        })
                    .ToListAsync();

                if (!swipers.Any())
                    return NotFound("Chưa có ai thích bạn !! Hãy chỉnh sửa hồ sơ cho ấn tượng hơn !!");

                return Ok(swipers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("swiped/{accountId}")]
        public async Task<IActionResult> GetSwipeds(int accountId)
        {
            try
            {
                var baseUrl = "https://res.cloudinary.com/dfvhhpkyg/image/upload/";

                var blockedAccountIds = await _db.Blocks
                    .Where(b => b.BlockerId == accountId)
                    .Select(b => b.BlockedUserId)
                    .ToListAsync();

                // Lấy danh sách bạn đã thích
                var swipeds = await _db.Swipes
                    .Where(s => s.AccountId == accountId && s.SwipeAction && !blockedAccountIds.Contains(s.AccountId))
                    .Join(_db.Profiles,
                        swipe => swipe.SwipedAccountId,
                        profile => profile.AccountId,
                        (swipe, profile) => new
                        {
                            AccountId = swipe.SwipedAccountId,
                            Avatar = profile.Avatar != null ? baseUrl + profile.Avatar : null,
                            profile.FullName,
                            IsMatched = _db.Swipes.Any(match =>
                                match.AccountId == swipe.SwipedAccountId &&
                                match.SwipedAccountId == accountId &&
                                match.SwipeAction)
                        })
                    .ToListAsync();

                if (!swipeds.Any())
                    return NotFound("Bạn chưa thích ai !!");

                return Ok(swipeds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("dislike/{accountId}")]
        public async Task<IActionResult> GetDisLike(int accountId)
        {
            try
            {
                var baseUrl = "https://res.cloudinary.com/dfvhhpkyg/image/upload/";

                var blockedAccountIds = await _db.Blocks
                    .Where(b => b.BlockerId == accountId)
                    .Select(b => b.BlockedUserId)
                    .ToListAsync();

                // Lấy danh sách bạn đã thích
                var swipeds = await _db.Swipes
                    .Where(s => s.AccountId == accountId && s.SwipeAction == false && !blockedAccountIds.Contains(s.AccountId))
                    .Join(_db.Profiles,
                        swipe => swipe.SwipedAccountId,
                        profile => profile.AccountId,
                        (swipe, profile) => new
                        {
                            AccountId = swipe.SwipedAccountId,
                            Avatar = profile.Avatar != null ? baseUrl + profile.Avatar : null,
                            profile.FullName,
                            IsMatched = _db.Swipes.Any(match =>
                                match.AccountId == swipe.SwipedAccountId &&
                                match.SwipedAccountId == accountId &&
                                match.SwipeAction)
                        })
                    .ToListAsync();

                if (!swipeds.Any())
                    return NotFound("Bạn chưa thích ai !!");

                return Ok(swipeds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }


        // GET: api/match/matches/{accountId}
        // Những người match (mutual like) với bạn
        [HttpGet("matches/{accountId}")]
        public async Task<IActionResult> GetMatches(int accountId)
        {
            try
            {
                var otherIds = await _db.Matches
                            .Where(m => m.User1Id == accountId || m.User2Id == accountId)
                            .Select(m => m.User1Id == accountId
                                ? m.User2Id
                                : m.User1Id)
                            .ToListAsync();

                if (!otherIds.Any())
                    return NotFound("Chưa có match nào.");

                return Ok(otherIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }


        // GET: api/match/count-today/{accountId}
        [HttpGet("SwipeCount/{accountId}")]
        public async Task<IActionResult> GetTodayMatchCount(int accountId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var tomorrow = today.AddDays(1);

                var count = await _db.Swipes
                                    .Where(m =>
                                        m.AccountId == accountId &&
                                        m.SwipedAt >= today &&
                                        m.SwipeAction &&
                                        m.SwipedAt < tomorrow)
                                    .CountAsync();

                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }

        }
    }
}
