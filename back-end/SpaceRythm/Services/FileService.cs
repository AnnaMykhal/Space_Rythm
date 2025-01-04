using Microsoft.AspNetCore.Mvc;
using SpaceRythm.Interfaces;

namespace SpaceRythm.Services
{
    public class FileService: IFileService
    {
        private readonly string _tracksDirectory;
        private readonly ITrackService _trackService;

        public FileService(ITrackService trackService, IWebHostEnvironment env)
        {
            _trackService = trackService;
            _tracksDirectory = Path.Combine(env.WebRootPath, "tracks");
        }

        public bool FileExists(string filePath)
        {
            return System.IO.File.Exists(filePath);
        }

        public async Task<string> GetFilePathAsync(int trackId)
        {
            var track = await _trackService.GetById(trackId);
            if (track == null)
            {
                return null;
            }

            return Path.Combine(_tracksDirectory, track.FilePath);
        }

        public async Task<PhysicalFileResult> GetFileAsync(string filePath, string mimeType, string fileName)
        {
            if (FileExists(filePath))
            {
                return new PhysicalFileResult(filePath, mimeType) { FileDownloadName = fileName };
            }

            throw new FileNotFoundException("File not found");
        }
    }
}
