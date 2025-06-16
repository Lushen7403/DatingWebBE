using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchLoveWeb.Models;
using MatchLoveWeb.Models.DTO;
using MatchLoveWeb.Services;
using MatchLoveWeb.SignaIR;
using Microsoft.AspNetCore.SignalR;

namespace MatchLoveWeb.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly DatingWebContext _db;
        private readonly PhotoService _photoService;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(
            DatingWebContext db,
            PhotoService photoService,
            IHubContext<ChatHub> hub)
        {
            _db = db;
            _photoService = photoService;
            _hub = hub;
        }

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversations(int userId)
        {
            var baseUrl = $"https://res.cloudinary.com/dfvhhpkyg/image/upload/";

            // Bước 1: Lấy danh sách các cuộc trò chuyện liên quan
            var convWithOther = _db.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Select(c => new
                {
                    c.Id,
                    OtherUserId = c.User1Id == userId ? c.User2Id : c.User1Id
                });

            // Bước 2: Lấy thông tin profile của người còn lại trong cuộc trò chuyện
            var result = await convWithOther
                .Join(
                    _db.Profiles,
                    conv => conv.OtherUserId,
                    prof => prof.AccountId,
                    (conv, prof) => new
                    {
                        conv.Id,
                        conv.OtherUserId,
                        prof.FullName,
                        prof.Avatar
                    })
                .ToListAsync();

            // Bước 3: Lấy tin nhắn cuối cùng cho mỗi conversation
            var conversationIds = result.Select(r => r.Id).ToList();

            var lastMessages = await _db.Messages
                .Where(m => conversationIds.Contains(m.ConversationId))
                .GroupBy(m => m.ConversationId)
                .Select(g => g.OrderByDescending(m => m.SentAt).FirstOrDefault())
                .ToListAsync();

            var unreadCounts = await _db.Messages
                        .Where(m => conversationIds.Contains(m.ConversationId)
                                 && m.SenderId != userId
                                 && m.IsRead == false)
                        .GroupBy(m => m.ConversationId)
                        .Select(g => new
                        {
                            ConversationId = g.Key,
                            Count = g.Count()
                        })
                        .ToListAsync();


            // Bước 4: Gộp dữ liệu lại
            var finalResult = result.Select(r =>
            {
                var lastMsg = lastMessages.FirstOrDefault(m => m.ConversationId == r.Id);
                string lastMessageText = "";
                DateTime? sentAt = DateTime.Now;
                if (lastMsg != null)
                {
                    if (!string.IsNullOrEmpty(lastMsg.MessageText))
                    {
                        lastMessageText = lastMsg.MessageText;
                        sentAt = lastMsg.SentAt;
                    }
                    else
                    {
                        if (userId != lastMsg.SenderId)
                        {
                            lastMessageText = $"{r.FullName} đã gửi ảnh";
                            sentAt = lastMsg.SentAt;
                        }
                        else
                        {
                            lastMessageText = "Bạn đã gửi ảnh";
                            sentAt = lastMsg.SentAt;
                        }

                        
                    }
                }

                return new ConversationUserDTO
                {
                    Id = r.Id,
                    OtherUserId = r.OtherUserId,
                    OtherUserName = r.FullName,
                    OtherUserAvatar = baseUrl + r.Avatar,
                    LastMessage = lastMessageText,
                    LastMessageAt = sentAt,
                    UnreadCount = unreadCounts.FirstOrDefault(u => u.ConversationId == r.Id)?.Count ?? 0
                };
            }).ToList();

            return Ok(finalResult);
        }



        // 3. Get messages
        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId, int page = 1, int pageSize = 50)
        {
            var msgs = await _db.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.MessageText,
                    m.SentAt,
                    Media = m.MessageMedia.Select(md => new { md.MediaUrl, md.MediaType })
                })
                .ToListAsync();
            return Ok(msgs);
        }

        [HttpPost("Upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
        [FromForm(Name = "file")] IFormFile file,
        [FromQuery] int messageId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file.");

            var message = await _db.Messages.FindAsync(messageId);
            if (message == null)
                return NotFound($"Không tìm thấy message với Id = {messageId}");

            try
            {
                var uploadResult = await _photoService.UploadMediaAsync(file);

                string mediaTypeCategory;
                if (file.ContentType.StartsWith("image/"))
                    mediaTypeCategory = "image";
                else if (file.ContentType.StartsWith("video/"))
                    mediaTypeCategory = "video";
                else if (file.ContentType.StartsWith("audio/"))
                    mediaTypeCategory = "audio";
                else
                    mediaTypeCategory = "unknown"; // hoặc throw nếu không cho phép

                var media = new MessageMedium
                {
                    MessageId = messageId,
                    MediaUrl = uploadResult.Url,
                    MediaType = mediaTypeCategory,
                    PublicId = uploadResult.PublicId,
                    UploadedAt = DateTime.UtcNow
                };

                _db.MessageMedia.Add(media);
                await _db.SaveChangesAsync();

                var msg = await _db.Messages.FindAsync(messageId);

                // Bây giờ build payload đúng y như SendMessage
                var payload = new
                {
                    id = msg.Id,
                    conversationId = msg.ConversationId,
                    senderId = msg.SenderId,
                    messageText = msg.MessageText,
                    sentAt = msg.SentAt,
                    media = new[]
                    {
                        new {
                            url  = media.MediaUrl,
                            type = media.MediaType
                        }
                    }
                            };

                // Broadcast payload
                var groupName = $"Conversation_{msg.ConversationId}";
                await _hub.Clients.Group(groupName)
                          .SendAsync("ReceiveMessage", payload);

                return Ok(new
                {
                    media.Id,
                    media.MediaUrl,
                    media.MediaType,
                    media.UploadedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi upload: {ex.Message}");
            }
        }


    }
}