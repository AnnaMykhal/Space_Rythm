using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Comment;
using System.Security.Claims;

namespace SpaceRythm.Controllers;


[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // Додати коментар
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Invalid comment data.");
        }

        await _commentService.AddComment(request.UserId, request.TrackId, request.Content);
        return Ok("Comment added successfully.");
    }


    // Отримати коментар по треку *
    [HttpGet("{trackId}")]
    public async Task<IActionResult> GetCommentsForTrack(int trackId)
    {
        var comments = await _commentService.GetCommentsForTrack(trackId);
        var response = comments.Select(c => new CommentResponse
        {
            CommentId = c.CommentId,
            UserId = c.UserId,
            UserName = c.UserName, 
            Content = c.Content,
            PostedDate = c.PostedDate,
            LikesCount = c.LikesCount
        });
        return Ok(response);
    }

    // Змінити коментар (може користувач тільки свій)
    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateComment([FromBody] UpdateCommentRequest request)
    {
        if (request == null || request.CommentId <= 0 || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Invalid update request.");
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }

        var comment = await _commentService.GetCommentByIdAsync(request.CommentId);
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }

        if (comment.UserId.ToString() != userId)
        {
            return Unauthorized("You are not authorized to edit this comment.");
        }
        var success = await _commentService.UpdateComment(request.CommentId, request.Content);
        if (!success)
        {
            return NotFound("Comment not found.");
        }

        return Ok("Comment updated successfully.");
    }

    // Видалити коментар (може користувач свій коментар або адмін)
    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var comment = await _commentService.GetCommentByIdAsync(commentId);
        if (comment == null)
        {
            return NotFound("Comment not found.");
        }

        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (comment.UserId.ToString() != userId && !isAdmin)
        {
            return Unauthorized("You are not authorized to delete this comment.");
        }

        await _commentService.DeleteComment(commentId);
        return Ok("Comment deleted successfully.");
    }

    private string? GetCurrentUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

}
