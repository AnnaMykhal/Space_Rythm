using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Entities;
using SpaceRythm.Models.Track;

namespace SpaceRythm.Interfaces;

public interface ITrackService
{
    //Task<Track> Create(CreateTrackRequest req);
    Task<PlainTrackResponse> UploadTrackAsync(IFormFile file, IFormFile image, CreateTrackRequest req, int userId);
    Task<IEnumerable<Track>> GetAll();
    Task<IEnumerable<PlainTrackResponse>> GetAllTracks(int? categoryId = null, int? artistId = null);
    Task<IEnumerable<Track>> GetByArtist(int artistId);

    Task<IEnumerable<Track>> GetByCategory(int categoryId);

    Task<Track> GetById(int id);
    Task Update(int id, UpdateTrackRequest req);
    Task<IEnumerable<PlainTrackResponse>> SearchByTitleOrArtist(string query, IUrlHelper urlHelper);
    Task<IEnumerable<Track>> GetPlainSongs();
    //Task<IEnumerable<PlainTrackResponse>> GetPlainSongs(IUrlHelper urlHelper);
    Task DeleteById(int id);
    Task<Track> GetTrackByTitle(string title);
    //bool FileExists(string filePath);

    Task<IEnumerable<PlainTrackResponse>> GetRecentTracks(int count = 10, IUrlHelper urlHelper = null);
    Task<IEnumerable<PlainTrackResponse>> GetRecommendedTracks(int userId, int count = 10, IUrlHelper urlHelper = null);
    Task<IEnumerable<PlainTrackResponse>> GetTopTracksByPlays(int count = 10, IUrlHelper urlHelper = null);
}
