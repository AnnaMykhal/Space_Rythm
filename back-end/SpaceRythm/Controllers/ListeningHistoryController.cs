using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Interfaces;
using System.Security.Claims;

namespace SpaceRythm.Controllers;

[ApiController]
[Route("api/listening-history")]
public class ListeningHistoryController : ControllerBase
{
    private readonly IListeningHistoryService _historyService;

    public ListeningHistoryController(IListeningHistoryService historyService)
    {
        _historyService = historyService;
    }

    // Додавання до історії прослуховувань *
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToHistory([FromBody] AddToHistoryRequest request)
    {
        if (request == null || request.UserId <= 0 || request.TrackId <= 0)
        {
            return BadRequest("Invalid request data.");
        }

        await _historyService.AddToListeningHistory(request.UserId, request.TrackId);
        return Ok("Track added to history.");
    }

    // Отримання історії прослуховувань *
    [Authorize]
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetHistory(int userId, [FromQuery] int? limit = null)
    {
        // Перевіряємо, чи користувач отримує власну історію
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || int.Parse(userIdClaim) != userId)
        {
            return Forbid("You can only access your own listening history.");
        }

        var history = await _historyService.GetListeningHistory(userId, limit);

        if (!history.Any())
        {
            return NotFound("No listening history found for the user.");
        }

        return Ok(history);
    }
}

public class AddToHistoryRequest
{
    public int UserId { get; set; }
    public int TrackId { get; set; }
}
