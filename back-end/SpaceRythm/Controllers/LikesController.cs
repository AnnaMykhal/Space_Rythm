using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Like;

namespace SpaceRythm.Controllers;

[Route("api/[controller]")]
[ApiController]

public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    // Додати лайк
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddLike([FromBody] CreateLikeRequest request)
    {
        if (request == null || request.UserId <= 0 || request.TrackId <= 0)
        {
            return BadRequest("Invalid request data.");
        }
        await _likeService.AddLike(request.UserId, request.TrackId);
        return Ok();
    }

    // Видалити лайк
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveLike(int userId, int trackId)
    {
        await _likeService.RemoveLike(userId, trackId);
        return Ok();
    }

    // Отримати кількість лайків по треку
    [HttpGet("{trackId}")]
    public async Task<IActionResult> GetLikesCountForTrack(int trackId)
    {
        var count = await _likeService.GetLikesCountForTrack(trackId);
        return Ok(count);
    }

    //[Authorize]
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetLikedTracks(int userId)
    {
        var likedTracks = await _likeService.GetLikedTracksByUser(userId);

        if (!likedTracks.Any())
        {
            return NotFound("No liked tracks found for the user.");
        }

        return Ok(likedTracks);
    }
}
