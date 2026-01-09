namespace Taxi_API.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string toPhone, string body);
    }
}
