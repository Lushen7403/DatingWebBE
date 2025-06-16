using MatchLoveWeb.Models;
using MatchLoveWeb.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MatchLoveWeb.SignaIR
{
    public class ChatHub : Hub
    {
        private readonly DatingWebContext _dbContext;
        private readonly IPresenceTracker _presence;
        private readonly TokenClassificationService _classifier;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ChatHub(
            DatingWebContext dbContext,
            IPresenceTracker presence,
            IHubContext<NotificationHub> notificationHub,
            TokenClassificationService classifier)
        {
            _dbContext = dbContext;
            _presence = presence;
            _notificationHub = notificationHub;
            _classifier = classifier;
        }

        private int GetUserId()
        {
            // Lấy UserId từ UserIdentifier đã cấu hình qua CustomUserIdProvider
            if (int.TryParse(Context.UserIdentifier, out int userId))
            {
                return userId;
            }

            throw new HubException("Không thể xác định user.");
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();

            await _presence.UserConnected(userId, Context.ConnectionId);
            await Clients.Caller.SendAsync("OnlineStatus", userId, true);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            await _presence.UserDisconnected(userId, Context.ConnectionId);

            if (!_presence.IsOnline(userId))
            {
                await Clients.All.SendAsync("OnlineStatus", userId, false);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public Task CheckOnline(int userId)
        {
            var isOnline = _presence.IsOnline(userId);
            return Clients.Caller.SendAsync("OnlineStatus", userId, isOnline);
        }

        public async Task JoinConversation(int conversationId)
        {
            try
            {
                var userId = GetUserId();

                var conversation = await _dbContext.Conversations.FindAsync(conversationId);
                if (conversation == null)
                {
                    throw new HubException("Cuộc hội thoại không tồn tại.");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(conversationId));

                var unreadMessages = await _dbContext.Messages
                                           .Where(m => m.ConversationId == conversationId
                                                       && m.SenderId != userId
                                                       && m.IsRead == false)
                                           .ToListAsync();

                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi JoinConversation: {ex.Message}");
                throw;
            }
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(conversationId));
        }

        public async Task<int> SendMessage(
            int conversationId,
            int senderId,
            string messageText,
            string[] mediaUrls,
            string[] mediaTypes)
        {

            var result = await _classifier.ClassifyAsync(messageText);
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                ToxicLevel = result?.level,
                ToxicTokenCount = result?.toxic_token_count ?? 0,
                ToxicDensity = result?.density ?? 0,
                HasHeavyWord = result?.has_heavy_word ?? false
            };

            // Tiếp tục xử lý bình thường nếu SAFE hoặc MILD

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            var mediaPayloads = mediaUrls.Select((url, i) => new {
                Url = url,
                Type = mediaTypes.Length > i ? mediaTypes[i] : "application/octet-stream"
            }).ToList();

            var payload = new
            {
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.MessageText,
                SentAt = message.SentAt,
                Media = mediaPayloads
            };

            await Clients.Group($"Conversation_{conversationId}")
                         .SendAsync("ReceiveMessage", payload);

            var conv = await _dbContext.Conversations.FindAsync(conversationId);
            if (conv != null)
            {
                // Xác định recipient
                var recipientId = conv.User1Id == senderId
                                  ? conv.User2Id
                                  : conv.User1Id;

                var senderProfile = await _dbContext.Profiles
                                            .FirstOrDefaultAsync(p => p.AccountId == senderId);
                var senderName = senderProfile != null
                    ? senderProfile.FullName   
                    : $"#{senderId}";

                var notif = new Notification
                {
                    UserId = recipientId,
                    NotificationTypeId = 1,                                
                    Content = $"Bạn có tin nhắn mới từ {senderName}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ReferenceId = conversationId                    
                };
                _dbContext.Notifications.Add(notif);

                if (result?.level == "MODERATE" || result?.level == "SEVERE")
                {
                    var systemWarning = new Notification
                    {
                        UserId = senderId,
                        NotificationTypeId = 2,
                        Content = "Tin nhắn của bạn có chứa từ ngữ nhạy cảm. Xin hãy giữ thái độ tôn trọng !!",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow,
                        ReferenceId = message.Id
                    };
                    _dbContext.Notifications.Add(systemWarning);

                    // Push SYSTEM warning realtime cho người gửi
                    await _notificationHub.Clients
                        .Group($"notif:{senderId}")
                        .SendAsync("ReceiveNotification", new
                        {
                            systemWarning.NotificationId,
                            systemWarning.Content,
                            systemWarning.CreatedAt,
                            systemWarning.NotificationTypeId,
                            systemWarning.ReferenceId
                        });
                }

                await _dbContext.SaveChangesAsync();

                // 4) Push realtime
                await _notificationHub.Clients
                    .Group($"notif:{recipientId}")
                    .SendAsync("ReceiveNotification", new
                    {
                        notif.NotificationId,
                        notif.Content,
                        notif.CreatedAt,
                        notif.NotificationTypeId,
                        notif.ReferenceId
                    });
            }


            return message.Id;
        }


        private static string GetGroupName(int conversationId) => $"Conversation_{conversationId}";
    }
}
