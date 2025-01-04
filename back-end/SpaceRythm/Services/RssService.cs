using SpaceRythm.Models_DTOs;
using System.ServiceModel.Syndication;
using System.Xml;



namespace SpaceRythm.Services;

public class RssService
{
    public async Task<List<MusicNews>> FetchRssFeedAsync(string rssUrl)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(rssUrl);

        using var stringReader = new StringReader(response);
        using var xmlReader = XmlReader.Create(stringReader);
        var feed = SyndicationFeed.Load(xmlReader);

        return feed.Items.Select(item => new MusicNews
        {
            Title = item.Title.Text,
            Link = item.Links.FirstOrDefault()?.Uri.ToString(), 
            Description = item.Summary?.Text ?? "No description available" 
        }).ToList();
    }
}
