using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Models_DTOs;
using SpaceRythm.Services;

namespace SpaceRythm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly RssService _rssService;

    public NewsController(RssService rssService)
    {
        _rssService = rssService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestNews()
    {
        //var rssUrl = "https://pitchfork.com/rss/news/";
        var rssUrl = "https://www.rollingstone.com/music/music-news/feed/";
        var news = await _rssService.FetchRssFeedAsync(rssUrl);

        if (news == null || !news.Any())
        {
            return NotFound("No news available.");
        }

        return Ok(news); 
    }
}
