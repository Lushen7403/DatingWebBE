using MatchLoveWeb.Models;
using MatchLoveWeb.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class MatchConditionController : ControllerBase
{
    private readonly DatingWebContext _context;

    public MatchConditionController(DatingWebContext context)
    {
        _context = context;
    }

    // GET: api/MatchCondition/account/5
    [HttpGet("{accountId}")]
    public async Task<ActionResult<MatchConditon>> GetByAccountId(int accountId)
    {
        var matchCondition = await _context.MatchConditons
            .FirstOrDefaultAsync(m => m.AccountId == accountId);

        if (matchCondition == null)
        {
            return NotFound();
        }

        return matchCondition;
    }



    // POST: api/MatchCondition
    [HttpPost("create")]
    public async Task<ActionResult<MatchConditon>> CreateMatchCondition([FromBody] MatchConditionDTO dto)
    {
        var matchCondition = new MatchConditon
        {
            AccountId = dto.AccountId,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            MaxDistanceKm = dto.MaxDistanceKm,
            GenderId = dto.GenderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MatchConditons.Add(matchCondition);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByAccountId), new { accountId = matchCondition.AccountId }, matchCondition);

    }

    // PUT: api/MatchCondition/account/5
    [HttpPut("update/account/{accountId}")]
    public async Task<IActionResult> UpdateByAccountId(int accountId, [FromBody] MatchConditionDTO dto)
    {
        var matchCondition = await _context.MatchConditons
            .FirstOrDefaultAsync(m => m.AccountId == accountId);

        if (matchCondition == null)
        {
            return NotFound();
        }

        matchCondition.MinAge = dto.MinAge;
        matchCondition.MaxAge = dto.MaxAge;
        matchCondition.MaxDistanceKm = dto.MaxDistanceKm;
        matchCondition.GenderId = dto.GenderId;
        matchCondition.UpdatedAt = DateTime.UtcNow;
        _context.Entry(matchCondition).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

}
