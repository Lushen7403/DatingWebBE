using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using MatchLoveWeb.Models;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminReportController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminReportController(DatingWebContext context)
        {
            _context = context;
        }

        // GET: api/AdminReport
        // GET: api/AdminReport
        [HttpGet]
        public async Task<ActionResult> GetReports(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Reports
                    .Include(r => r.User)             // Người báo cáo
                    .Include(r => r.ReportedUser)     // Người bị báo cáo
                    .Include(r => r.ReportedType)     // Loại vi phạm
                    .OrderByDescending(r => r.ReportAt)
                    .AsQueryable();

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var reports = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,                                   // id của report
                        ReporterId = r.UserId,            // (tuỳ chọn) id của người báo cáo
                        Reporter = r.User.UserName,
                        ReportedUserId = r.ReportedUserId,    // id của người bị báo cáo
                        ReportedIsBanned = r.ReportedUser.IsBanned,
                        Reported = r.ReportedUser.UserName,
                        Violation = r.ReportedType.ReportTypeName,
                        Content = r.Content,
                        ReportedAt = r.ReportAt
                    })
                    .ToListAsync();


                return Ok(new
                {
                    reports,
                    totalItems,
                    totalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}