namespace Taxi_API.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}