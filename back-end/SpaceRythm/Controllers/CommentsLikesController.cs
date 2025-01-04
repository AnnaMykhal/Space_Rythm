using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Like;
using Microsoft.AspNetCore.Authorization;


namespace SpaceRythm.Controllers;

[ApiController]
[Route("api/comments/likes")]
public class CommentsLikesController : ControllerBase
{
    private readonly ICommentLikeService _commentLikeService;

    public CommentsLikesController(ICommentLikeService commentLikeService)
    {
        _commentLikeService = commentLikeService;
    }

    // Додаємо лайк до коментаря
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddLike([FromBody] CreateCommentLikeRequest request)
    {
        if (request == null || request.UserId <= 0 || request.CommentId <= 0)
        {
            return BadRequest("Invalid request data.");
        }
        await _commentLikeService.AddLike(request.UserId, request.CommentId);
        return Ok();
    }

    // Видаляємо лайк з коментаря
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveLike(int userId, int commentId)
    {
        await _commentLikeService.RemoveLike(userId, commentId);
        return Ok();
    }

    // Отримуємо кількість лайків
    [HttpGet("{commentId}")]
    public async Task<IActionResult> GetLikesCountForComment(int commentId)
    {
        var count = await _commentLikeService.GetLikesCountForComment(commentId);
        return Ok(count);
    }
}
