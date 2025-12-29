using Taxi_API.Models;

namespace Taxi_API.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}