using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Models.Category;
using SpaceRythm.Interfaces;
using SpaceRythm.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace SpaceRythm.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrackCategoriesController : ControllerBase
{
    private readonly ITrackCategoryService _categoryService;

    public TrackCategoriesController(ITrackCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IEnumerable<TrackCategory>> GetAll()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}/";
        var categories = await _categoryService.GetAll();

        return categories.Select(c => new TrackCategory
        {
            Id = c.Id,
            Category = c.Category,
            ImageUrl = $"{baseUrl}{c.ImageUrl}" 
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrackCategory>> GetById(int id)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}/";
        var category = await _categoryService.GetById(id);

        if (category == null)
        {
            return NotFound();
        }

        category.ImageUrl = $"{baseUrl}{category.ImageUrl}";

        return Ok(category);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCategoryRequest req)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized("User is not authenticated.");
        }
        if (req.Image != null && req.Image.Length > 0)
        {
            var imageUrl = await _categoryService.SaveImage(req.Image);
        }
        await _categoryService.Create(req);
        return Ok();
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            return Unauthorized("You are not authorized to delete this track category.");
        }

        await _categoryService.DeleteById(id);
        return Ok("Category deleted successfully.");
    }

    private string? GetCurrentUserId()
    {
        return User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
