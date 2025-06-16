using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using MatchLoveWeb.Models;             
using MatchLoveWeb.Models.DTO;         
using MatchLoveWeb.Models;             
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.SignaIR
{
    public class NotificationHub : Hub
    {
        private readonly DatingWebContext _db;

        public NotificationHub(DatingWebContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Khi client kết nối, tự động join vào group riêng của user
        /// (GroupName = "notif:{userId}")
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    GetGroupName(userId));
            }
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Khi client ngắt kết nối, auto leave group
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    GetGroupName(userId));
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Client có thể gọi để đánh dấu một notification đã đọc.
        /// Sau đó hub sẽ broadcast lại confirm về client.
        /// </summary>
        public async Task MarkAsRead(int notificationId)
        {
            var notif = await _db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notif != null && notif.IsRead == false)
            {
                notif.IsRead = true;
                _db.Notifications.Update(notif);
                await _db.SaveChangesAsync();

                // Gửi event confirm về chính client gọi
                await Clients.Caller.SendAsync("NotificationMarked", notificationId);
            }
        }

        /// <summary>
        /// Tạo group name cố định dựa trên userId
        /// </summary>
        private static string GetGroupName(string userId) =>
            $"notif:{userId}";
    }
}
