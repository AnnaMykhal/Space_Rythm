using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Entities;
using SpaceRythm.Models.Artist;

namespace SpaceRythm.Interfaces;

public interface IArtistService
{
    Task Create(IFormFile image, CreateArtistRequest req);

    Task Delete(int id);

    Task<IEnumerable<Artist>> GetAll();

    Task<Artist> GetById(int id);

    Task<IEnumerable<Artist>> GetByCategory(int category);

    Task UpdateAsync(int id, UpdateArtistRequest req);
    //Task<IEnumerable<PlainArtistResponse>> GetPlainArtists(IUrlHelper urlHelper);
    Task<IEnumerable<PlainArtistResponse>> GetPlainArtists(IEnumerable<Entities.Artist> artists);
}
