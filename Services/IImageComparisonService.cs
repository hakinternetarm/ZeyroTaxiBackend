namespace Taxi_API.Services
{
    public interface IImageComparisonService
    {
        /// <summary>
        /// Compare two images containing faces and return similarity score (0..1) and whether they match based on internal threshold.
        /// </summary>
        Task<(double score, bool match)> CompareFacesAsync(string imagePath1, string imagePath2);

        /// <summary>
        /// Analyze car exterior images and return damage score (0..1) and whether the car is considered OK (no significant damage).
        /// </summary>
        Task<(double score, bool ok)> CheckCarDamageAsync(IEnumerable<string> exteriorImagePaths);
    }
}