using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminMessageController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminMessageController(DatingWebContext context)
        {
            _context = context;
        }
        [HttpGet("toxic-messages")]
        public async Task<IActionResult> GetToxicMessagesByUser(int page = 1, int pageSize = 10)
        {
            var toxicLevels = new[] { "MILD", "MODERATE", "SEVERE" };

            var query = _context.Messages
                .Where(m => toxicLevels.Contains(m.ToxicLevel))
                .GroupBy(m => m.SenderId)
                .Select(g => new
                {
                    SenderId = g.Key,
                    TotalToxicMessages = g.Count(),
                    LatestToxicMessageTime = g.Max(m => m.SentAt),
                    Messages = g.OrderByDescending(m => m.SentAt).Select(m => new
                    {
                        m.Id,
                        m.MessageText,
                        m.ToxicLevel,
                        m.SentAt
                    }).ToList()
                });

            var groupedData = await query
                .OrderByDescending(g => g.LatestToxicMessageTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Join với bảng Account để lấy UserName
            var senderIds = groupedData.Select(g => g.SenderId).ToList();
            var accounts = await _context.Accounts
                .Where(a => senderIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.UserName);

            var result = groupedData.Select(g => new
            {
                UserName = accounts.ContainsKey(g.SenderId) ? accounts[g.SenderId] : $"#{g.SenderId}",
                g.TotalToxicMessages,
                g.LatestToxicMessageTime,
                g.Messages
            });

            return Ok(result);
        }




    }
}
