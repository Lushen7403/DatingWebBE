using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public VoucherController(DatingWebContext context)
        {
            _context = context;
        }

        [HttpGet("voucher")]
        public async Task<IActionResult> GetActiveVouchers()
        {
            var now = DateTime.Now;

            var vouchers = await _context.Vouchers
                .Where(v => v.IsActive == true && v.StartDate <= now && v.EndDate >= now)
                .Select(v => new
                {
                    v.Id,
                    v.Code,
                    v.DiscountPercent
                })
                .ToListAsync();

            return Ok(vouchers);
        }
    }
}
