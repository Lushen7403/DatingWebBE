using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchLoveWeb.Models;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiamondController : ControllerBase
    {
        private readonly DatingWebContext _context; // Thay YourDbContext bằng tên DbContext của bạn

        public DiamondController(DatingWebContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllActive")]
        public async Task<ActionResult<IEnumerable<RechargePackage>>> GetActiveRechargePackages()
        {
            try
            {
                var packages = await _context.RechargePackages
                    .Where(p => p.IsActivate == true) 
                    .OrderBy(p => p.Price)
                    .Select(p => new
                    {
                        id = p.Id,
                        price = p.Price,
                        diamond = p.DiamondCount
                    })
                    .ToListAsync();

                return Ok(packages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("balance/{accountId}")]
        public async Task<ActionResult<int>> GetDiamondBalance(int accountId)
        {
            try
            {
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == accountId);

                if (account == null)
                {
                    return NotFound($"Không tìm thấy tài khoản với ID: {accountId}");
                }

                return Ok(account.DiamondCount);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

    }
}