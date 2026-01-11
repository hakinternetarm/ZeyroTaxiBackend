namespace Taxi_API.Services
{
    public interface IOcrService
    {
        // Extracts text from an image file path. Returns null if OCR is not available or failed.
        Task<string?> ExtractTextAsync(string imagePath, string lang = "eng");
    }
}
