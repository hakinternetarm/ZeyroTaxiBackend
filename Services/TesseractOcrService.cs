using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace Taxi_API.Services
{
    // OCR service using Tesseract.NET (Tesseract NuGet). Requires tessdata files available at configured path.
    public class TesseractOcrService : IOcrService
    {
        private readonly ILogger<TesseractOcrService> _logger;
        private readonly string _tessDataPath;

        public TesseractOcrService(IConfiguration config, ILogger<TesseractOcrService> logger)
        {
            _logger = logger;
            _tessDataPath = config["Ocr:TessdataPath"] ?? "tessdata";
        }

        public async Task<string?> ExtractTextAsync(string imagePath, string lang = "eng")
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(_tessDataPath))
                    {
                        _logger.LogWarning("Tessdata path not found: {path}", _tessDataPath);
                        return (string?)null;
                    }

                    using var engine = new TesseractEngine(_tessDataPath, lang, EngineMode.Default);
                    using var img = Pix.LoadFromFile(imagePath);
                    using var page = engine.Process(img);
                    var text = page.GetText();
                    return text;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Tesseract OCR failed for {image}", imagePath);
                    return (string?)null;
                }
            });
        }
    }
}
