using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public ReportController(DatingWebContext context)
        {
            _context = context;
        }

        [HttpPost("report")]
        public async Task<IActionResult> CreateReport(
            [FromQuery] int userId,
            [FromQuery] int reportedUserId,
            [FromQuery] int reportedTypeId,
            [FromQuery] string content)
        {
            var report = new Report
            {
                UserId = userId,
                ReportedUserId = reportedUserId,
                ReportedTypeId = reportedTypeId,
                Content = content,
                IsChecked = false,
                ReportAt = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, new
            {
                report.Id,
                report.UserId,
                report.ReportedUserId,
                report.ReportedTypeId,
                report.Content,
                report.IsChecked,
                report.ReportAt
            });
        }

        // GET: api/ReportType
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var types = await _context.ReportTypes
                .Select(t => new
                {
                    t.Id,
                    t.ReportTypeName
                })
                .ToListAsync();

            return Ok(types);
        }
    }
}
