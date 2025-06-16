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
    public class AdminPackageController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public AdminPackageController(DatingWebContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách gói nạp có phân trang.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPackages(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _context.RechargePackages
                    .OrderBy(rp => rp.Id)
                    .AsQueryable();

                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var packages = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(rp => new
                    {
                        rp.Id,
                        rp.Price,
                        rp.DiamondCount,
                        rp.Description,
                        rp.IsActivate
                    })
                    .ToListAsync();

                return Ok(new
                {
                    packages,
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
        /// Tạo mới gói nạp.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePackage([FromBody] RechargePackage input)
        {
            if (input == null)
                return BadRequest("Body không hợp lệ.");

            try
            {
                var pkg = new RechargePackage
                {
                    Price = input.Price,
                    DiamondCount = input.DiamondCount,
                    Description = input.Description,
                    IsActivate = input.IsActivate
                };

                _context.RechargePackages.Add(pkg);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetPackages),
                    new { id = pkg.Id },
                    new
                    {
                        pkg.Id,
                        pkg.Price,
                        pkg.DiamondCount,
                        pkg.Description,
                        pkg.IsActivate
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật gói nạp theo id.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] RechargePackage input)
        {
            if (input == null)
                return BadRequest("Body không hợp lệ.");

            try
            {
                var pkg = await _context.RechargePackages.FindAsync(id);
                if (pkg == null)
                    return NotFound($"Không tìm thấy gói nạp với ID: {id}");

                pkg.Price = input.Price;
                pkg.DiamondCount = input.DiamondCount;
                pkg.Description = input.Description;
                pkg.IsActivate = input.IsActivate;

                _context.RechargePackages.Update(pkg);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    pkg.Id,
                    pkg.Price,
                    pkg.DiamondCount,
                    pkg.Description,
                    pkg.IsActivate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa gói nạp theo id.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            try
            {
                var pkg = await _context.RechargePackages.FindAsync(id);
                if (pkg == null)
                    return NotFound($"Không tìm thấy gói nạp với ID: {id}");

                _context.RechargePackages.Remove(pkg);
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
