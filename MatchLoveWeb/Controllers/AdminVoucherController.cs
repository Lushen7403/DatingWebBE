using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatchLoveWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminVoucherController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminVoucherController(DatingWebContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách voucher có phân trang.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetVouchers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.Vouchers
                    .OrderBy(v => v.Id)
                    .AsQueryable();

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var vouchers = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(v => new
                    {
                        v.Id,
                        v.Code,
                        v.Description,
                        v.DiscountPercent,
                        v.StartDate,
                        v.EndDate,
                        v.IsActive
                    })
                    .ToListAsync();

                return Ok(new
                {
                    vouchers,
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

        /// <summary>
        /// Tạo mới voucher.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateVoucher([FromBody] Voucher input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Code))
                return BadRequest("Vui lòng truyền đầy đủ Code và các trường bắt buộc.");

            if (input.StartDate >= input.EndDate)
                return BadRequest("StartDate phải trước EndDate.");

            try
            {
                var voucher = new Voucher
                {
                    Code = input.Code.Trim(),
                    Description = input.Description,
                    DiscountPercent = input.DiscountPercent,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    IsActive = input.IsActive ?? true
                };

                _context.Vouchers.Add(voucher);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetVouchers),
                    new { id = voucher.Id },
                    new
                    {
                        voucher.Id,
                        voucher.Code,
                        voucher.Description,
                        voucher.DiscountPercent,
                        voucher.StartDate,
                        voucher.EndDate,
                        voucher.IsActive
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật voucher theo id.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] Voucher input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Code))
                return BadRequest("Vui lòng truyền đầy đủ Code và các trường bắt buộc.");

            if (input.StartDate >= input.EndDate)
                return BadRequest("StartDate phải trước EndDate.");

            try
            {
                var voucher = await _context.Vouchers.FindAsync(id);
                if (voucher == null)
                    return NotFound($"Không tìm thấy voucher với ID: {id}");

                voucher.Code = input.Code.Trim();
                voucher.Description = input.Description;
                voucher.DiscountPercent = input.DiscountPercent;
                voucher.StartDate = input.StartDate;
                voucher.EndDate = input.EndDate;
                voucher.IsActive = input.IsActive ?? voucher.IsActive;

                _context.Vouchers.Update(voucher);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    voucher.Id,
                    voucher.Code,
                    voucher.Description,
                    voucher.DiscountPercent,
                    voucher.StartDate,
                    voucher.EndDate,
                    voucher.IsActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa voucher theo id.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            try
            {
                var voucher = await _context.Vouchers.FindAsync(id);
                if (voucher == null)
                    return NotFound($"Không tìm thấy voucher với ID: {id}");

                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();

                return NoContent(); // 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
