using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminPaymentController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminPaymentController(DatingWebContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Payments
                    .Include(p => p.Account)
                    .Include(p => p.Package)
                    .Include(p => p.Voucher)
                    .OrderByDescending(p => p.CreatedAt)
                    .AsQueryable();

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var payments = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.AccountId,
                        p.PackageId,
                        p.VoucherId,
                        p.AmountBefore,
                        p.DiscountAmount,
                        p.AmountAfter,
                        p.Status,
                        p.VnpTxnRef,
                        p.VnpResponseCode,
                        p.VnpTransactionStatus,
                        p.VnpBankCode,
                        p.VnpBankTranNo,
                        p.VnpPayDate,
                        p.VnpSecureHash,
                        p.CreatedAt,
                        p.UpdatedAt,

                        Account = new
                        {
                            p.Account.UserName,
                            p.Account.DiamondCount
                        },
                        Package = new
                        {
                            p.Package.Price,
                            p.Package.DiamondCount
                        },
                        Voucher = p.Voucher == null ? null : new
                        {
                            p.Voucher.Code,
                            p.Voucher.DiscountPercent
                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    payments,
                    totalItems,
                    totalPages,
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

    }
}
