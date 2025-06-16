using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminDashController(DatingWebContext context)
        {
            _context = context;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDashboardData()
        {
            var now = DateTime.Now;
            var from7 = now.AddDays(-6).Date; // 7 ngày, bao gồm hôm nay

            // 1. Tổng người dùng
            var totalUsers = await _context.Accounts.CountAsync();

            // 2. Tổng thanh toán 7 ngày
            var totalPayments = await _context.Payments
                .Where(p => p.Status)
                .SumAsync(p => p.AmountAfter);

            // 3. Đăng ký mới theo ngày (7 ngày gần nhất)
            var regs = await _context.Accounts
                .Where(u => u.CreatedAt >= from7)
                .GroupBy(u => u.CreatedAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 4. Match thành công theo ngày (7 ngày gần nhất)
            var matches = await _context.Matches
                .Where(m => m.MatchedAt.HasValue && m.MatchedAt.Value >= from7)
                .GroupBy(m => m.MatchedAt.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // 5. Doanh thu theo ngày (7 ngày gần nhất)
            var revenue = await _context.Payments
                .Where(p => p.Status && p.CreatedAt >= from7)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Sum = g.Sum(p => p.AmountAfter) })
                .ToListAsync();

            // Chuẩn hóa thành mảng 7 ngày liền kề
            List<string> labels = Enumerable
                .Range(0, 7)
                .Select(i => from7.AddDays(i).ToString("ddd", new CultureInfo("vi-VN")))
                .ToList();

            Dictionary<string, int> regMap = regs.ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);
            Dictionary<string, int> matchMap = matches.ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);
            Dictionary<string, decimal> revMap = revenue.ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Sum);

            var newRegistrations = labels
                .Select((lbl, i) =>
                {
                    var d = from7.AddDays(i).ToString("yyyy-MM-dd");
                    return regMap.ContainsKey(d) ? regMap[d] : 0;
                })
                .ToList();

            var matchedCounts = labels
                .Select((lbl, i) =>
                {
                    var d = from7.AddDays(i).ToString("yyyy-MM-dd");
                    return matchMap.ContainsKey(d) ? matchMap[d] : 0;
                })
                .ToList();

            var revenues = labels
                .Select((lbl, i) =>
                {
                    var d = from7.AddDays(i).ToString("yyyy-MM-dd");
                    return revMap.ContainsKey(d) ? revMap[d] : 0m;
                })
                .ToList();

            return Ok(new
            {
                totalUsers,
                totalPayments,
                chart = new
                {
                    labels,              
                    newRegistrations,    
                    matchedCounts,       
                    revenues            
                }
            });
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyDashboardData()
        {
            var now = DateTime.Now.Date;
            var start = now.AddDays(-7 * 6); // 7 tuần: hiện tại + 6 trước

            var weeks = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var wkStart = start.AddDays(i * 7);
                    var weekOfMonth = (wkStart.Day - 1) / 7 + 1;
                    var label = $"T{weekOfMonth}/Th{wkStart.Month}";
                    return new { Label = label, Start = wkStart, End = wkStart.AddDays(7) };
                }).ToList();

            var newRegs = new List<int>();
            var matches = new List<int>();
            var revenues = new List<decimal>();

            foreach (var w in weeks)
            {
                newRegs.Add(await _context.Accounts
                    .CountAsync(u => u.CreatedAt >= w.Start && u.CreatedAt < w.End));

                matches.Add(await _context.Matches
                    .CountAsync(m => m.MatchedAt.HasValue && m.MatchedAt.Value >= w.Start && m.MatchedAt.Value < w.End));

                revenues.Add(await _context.Payments
                    .Where(p => p.Status && p.CreatedAt >= w.Start && p.CreatedAt < w.End)
                    .SumAsync(p => p.AmountAfter));
            }

            var totalUsers = await _context.Accounts.CountAsync();
            var totalPayments = await _context.Payments
                .Where(p => p.Status)
                .SumAsync(p => p.AmountAfter);

            return Ok(new
            {
                totalUsers,
                totalPayments,
                chart = new
                {
                    labels = weeks.Select(w => w.Label).ToList(),
                    newRegistrations = newRegs,
                    matchedCounts = matches,
                    revenues = revenues
                }
            });
        }
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyDashboardData()
        {
            var now = DateTime.Now;
            var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-6); // 7 tháng

            var months = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var mStart = startMonth.AddMonths(i);
                    var label = $"Th{mStart.Month}/{mStart.Year}";
                    return new { Label = label, Start = mStart, End = mStart.AddMonths(1) };
                }).ToList();

            var newRegs = new List<int>();
            var matches = new List<int>();
            var revenues = new List<decimal>();

            foreach (var m in months)
            {
                newRegs.Add(await _context.Accounts
                    .CountAsync(u => u.CreatedAt >= m.Start && u.CreatedAt < m.End));

                matches.Add(await _context.Matches
                    .CountAsync(mt => mt.MatchedAt.HasValue && mt.MatchedAt.Value >= m.Start && mt.MatchedAt.Value < m.End));

                revenues.Add(await _context.Payments
                    .Where(p => p.Status && p.CreatedAt >= m.Start && p.CreatedAt < m.End)
                    .SumAsync(p => p.AmountAfter));
            }

            var totalUsers = await _context.Accounts.CountAsync();
            var totalPayments = await _context.Payments
                .Where(p => p.Status)
                .SumAsync(p => p.AmountAfter);

            return Ok(new
            {
                totalUsers,
                totalPayments,
                chart = new
                {
                    labels = months.Select(m => m.Label).ToList(),
                    newRegistrations = newRegs,
                    matchedCounts = matches,
                    revenues = revenues
                }
            });
        }



    }
}
