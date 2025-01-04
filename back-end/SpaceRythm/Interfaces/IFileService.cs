using Microsoft.AspNetCore.Mvc;

namespace SpaceRythm.Interfaces;

public interface IFileService
{
    public bool FileExists(string filePath); 
    public Task<string> GetFilePathAsync(int trackId);
    public Task<PhysicalFileResult> GetFileAsync(string filePath, string mimeType, string fileName);
}

