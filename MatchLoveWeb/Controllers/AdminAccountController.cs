using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AdminAccountController : ControllerBase
{
    private readonly DatingWebContext _context;

    public AdminAccountController(DatingWebContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isBanned,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Accounts
                .Include(a => a.Role)
                .Where(a => a.RoleId != 1)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a =>
                    a.UserName.Contains(searchTerm) ||
                    a.Email.Contains(searchTerm));
            }

            // Apply ban filter
            if (isBanned.HasValue)
            {
                query = query.Where(a => a.IsBanned == isBanned.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Get paginated results
            var accounts = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.RoleId,
                    a.UserName,
                    a.Email,
                    a.CreatedAt,
                    a.UpdatedAt,
                    a.IsBanned,
                    a.DiamondCount
                })
                .ToListAsync();

            return Ok(new
            {
                accounts,
                totalCount,
                totalPages,
                currentPage = page
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/ban")]
    public async Task<IActionResult> BanAccount(int id)
    {
        try
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound($"Account with ID {id} not found");
            }

            account.IsBanned = true;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Taì khoản đã được khóa thành công !!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/unban")]
    public async Task<IActionResult> UnbanAccount(int id)
    {
        try
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound($"Account with ID {id} not found");
            }

            account.IsBanned = false;
            account.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tài khoản đã được mở khóa thành công !!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }
}