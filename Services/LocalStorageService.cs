using Microsoft.Extensions.Configuration;

namespace Taxi_API.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _root;

        public LocalStorageService(IConfiguration config)
        {
            _root = config["Storage:Path"] ?? "Storage";
            if (!Directory.Exists(_root)) Directory.CreateDirectory(_root);
        }

        public async Task<string> SaveFileAsync(Stream stream, string fileName)
        {
            var safe = Path.GetRandomFileName();
            var full = Path.Combine(_root, safe + Path.GetExtension(fileName));
            using var fs = File.Create(full);
            await stream.CopyToAsync(fs);
            return full;
        }

        public Task DeleteFileAsync(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }
}