namespace Taxi_API.Services
{
    public interface IStorageService
    {
        Task<string> SaveFileAsync(Stream stream, string fileName);
        Task DeleteFileAsync(string path);
    }
}