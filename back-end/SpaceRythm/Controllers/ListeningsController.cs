using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Listening;
using SpaceRythm.Services;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Claims;
using ActiveUp.Net.Security.OpenPGP.Packets;

namespace SpaceRythm.Controllers;

[ApiController]
[Route("api/listenings")]
public class ListeningsController : ControllerBase
{
    private readonly IListeningService _listeningService;

    public ListeningsController(IListeningService listeningService)
    {
        _listeningService = listeningService;
    }

    // Додати простуховування *
    [HttpPost]
    public async Task<IActionResult> AddListening([FromBody] CreateListeningRequest request)
    {
        try
        {
            int? userId = null;
           
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                userId = int.Parse(userIdClaim);
            }

            await _listeningService.AddListening(userId, request.TrackId);
            return Ok("Listening recorded successfully.");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Отримати кількість прослуховувань *
    [HttpGet("count/{trackId}")]
    public async Task<IActionResult> GetListeningsCountForTrack(int trackId)
    {
        var count = await _listeningService.GetListeningsCountForTrack(trackId);

        if (count == 0)
        {
            return NotFound($"No listenings found for track with ID {trackId}");
        }

        return Ok(count);
    }
}
