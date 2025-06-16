using MatchLoveWeb.Models;
using MatchLoveWeb.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class LocationController : ControllerBase
{
    private readonly DatingWebContext _context;

    public LocationController(DatingWebContext context)
    {
        _context = context;
    }

    // POST: api/Location
    [HttpPost("create")]
    public async Task<IActionResult> CreateLocation([FromBody] LocationDTO dto)
    {
        var location = new Location
        {
            AccountId = dto.AccountId,
            Longitude = dto.Longitude,
            Latitude = dto.Latitude,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
    }

    // GET: api/Location/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Location>> GetLocation(int id)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(p => p.AccountId == id);
        if (location == null) return NotFound();
        return location;
    }

    // PUT: api/Location/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationDTO dto)
    {
        var location = await _context.Locations.FirstOrDefaultAsync(p => p.AccountId == id);
        if (location == null)
        {
            return NotFound();
        }

        location.Longitude = dto.Longitude;
        location.Latitude = dto.Latitude;
        location.UpdatedAt = DateTime.UtcNow;

        _context.Update(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }


}
