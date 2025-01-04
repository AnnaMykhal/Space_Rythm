using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Attributes;
using SpaceRythm.Data;
using SpaceRythm.Entities;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Artist;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace SpaceRythm.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ArtistsController : ControllerBase
{
    private readonly MyDbContext _context;
    private readonly IArtistService _artistService;
    private readonly IUserService _userService;

    public ArtistsController(MyDbContext context, IArtistService artistService, IUserService userService)
    {
        _context=context;
        _artistService = artistService;
        _userService = userService;
    }

    // Отримання виконавця по id тільки ім'я *
    [HttpGet("name/{id}")]
    public async Task<IActionResult> GetArtistName(int id)
    {
        var res = await _artistService.GetById(id);
        if (res == null)
            return NotFound();

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            MaxDepth = 64
        };
        return Ok(new { name = res.Name });
    }

    // Отримання списку виконавців *
    [HttpGet("plain")]
    public async Task<IActionResult> GetPlainArtists()
    {
        var artists = await _context.Artists.ToListAsync();

        var res = await _artistService.GetPlainArtists(artists);

        return Ok(res);
    }

    // Отримання виконавців за категорією(categoryId > 0), id виконавця (singerId > 0) або всі виконавці *
    [HttpGet]
    public async Task<IActionResult> Get(int categoryId, int singerId)
    {
        if (singerId > 0)
        {
            var res = await _artistService.GetById(singerId);

            if (res == null)
                return NotFound();

            return Ok(res);
        }

        if (categoryId > 0)
        {
            var res = await _artistService.GetByCategory(categoryId);
            return Ok(res);
        }

        var response = await _artistService.GetAll();
        return Ok(response);
    }

    // Створення нового виконавця *
    [HttpPost]
    public async Task<StatusCodeResult> Create([FromForm] IFormFile image, [FromForm] CreateArtistRequest req)
    {
        await _artistService.Create(image, req);
        return new StatusCodeResult(201);
    }

    // Оновлення виконавця *
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateArtistRequest req)
    {
        try
        {
            await _artistService.UpdateAsync(id, req);

            return Ok(new { message = "Artist updated successfully" });
        }
        catch (Exception ex)
        {
            if (ex.Message == "Artist not found")
            {
                return NotFound(new { error = ex.Message });
            }

            return BadRequest(new { error = "An error occurred", details = ex.Message });
        }
    }

    // Видалення виконавця *
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound(new { message = "Artist not found" });
            }

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Artist deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "An error occurred while deleting the artist", details = ex.Message });
        }
    }
}
