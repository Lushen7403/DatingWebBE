using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchLoveWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        private readonly DatingWebContext _context;

        public BlockController(DatingWebContext context)
        {
            _context = context;
        }

        [HttpPost("block")]
        public async Task<IActionResult> Block(int accountId, int blockedAccountId)
        {
           var block = new Block();
            block.BlockerId = accountId;
            block.BlockedUserId = blockedAccountId;
            block.BlockAt = DateTime.UtcNow;
            _context.Blocks.Add(block);
            await _context.SaveChangesAsync();

            var response = new
            {
                block.BlockerId,
                block.BlockedUserId,
                block.BlockAt
            };

            return StatusCode(StatusCodes.Status201Created, new
            {
                block.BlockerId,
                block.BlockedUserId,
                block.BlockAt
            });

        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] int accountId)
        {
            var baseUrl = $"https://res.cloudinary.com/dfvhhpkyg/image/upload/";
            var list = await _context.Blocks
                .Where(b => b.BlockerId == accountId)
                .OrderByDescending(b => b.BlockAt)
                .Select(b => new
                {
                    b.BlockerId,
                    b.BlockedUserId,
                    b.BlockAt,
                    Profile = _context.Profiles
                        .Where(p => p.AccountId == b.BlockedUserId)
                        .Select(p => new
                        {
                            Avatar = baseUrl + p.Avatar,
                            p.FullName
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(list);
        }

        // DELETE: api/Block?accountId=123&blockedAccountId=456
        [HttpDelete("unBlock")]
        public async Task<IActionResult> Unblock(int accountId, int blockedAccountId)
        {
            var block = await _context.Blocks
                .FirstOrDefaultAsync(b =>
                    b.BlockerId == accountId &&
                    b.BlockedUserId == blockedAccountId
                );

            if (block == null)
                return NotFound();

            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("isBlock")]
        public async Task<IActionResult> IsBlocked(int accountId, int blockedAccountId)
        {
            var isBlocked = await _context.Blocks
                .AnyAsync(b =>
                    b.BlockerId == accountId &&
                    b.BlockedUserId == blockedAccountId
                );

            return Ok(isBlocked);
        }
    }
}
