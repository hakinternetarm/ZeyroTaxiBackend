using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taxi_API.Data;
using Taxi_API.DTOs;
using Taxi_API.Models;
using Taxi_API.Services;

namespace Taxi_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _email;

        public AuthController(AppDbContext db, ITokenService tokenService, IEmailService email)
        {
            _db = db;
            _tokenService = tokenService;
            _email = email;
        }

        [HttpPost("request-code")]
        public async Task<IActionResult> RequestCode([FromBody] RequestCodeRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Phone)) return BadRequest("Phone is required");

            var code = new Random().Next(100000, 999999).ToString();
            var session = new AuthSession
            {
                Id = Guid.NewGuid(),
                Phone = req.Phone,
                Code = code,
                Verified = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            _db.AuthSessions.Add(session);
            await _db.SaveChangesAsync();

            // send code via email/sms. For simplicity use email service with phone@example.com
            await _email.SendAsync(req.Phone + "@example.com", "Your login code", $"Your code is: {code}");

            return Ok(new { AuthSessionId = session.Id.ToString() });
        }

        [HttpPost("auth")] // combined login/register using session id + code
        public async Task<IActionResult> Auth([FromBody] AuthRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.AuthSessionId) || string.IsNullOrWhiteSpace(req.Code))
                return BadRequest("AuthSessionId and Code are required");

            if (!Guid.TryParse(req.AuthSessionId, out var sessionId)) return BadRequest("Invalid AuthSessionId");

            var session = await _db.AuthSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return BadRequest("Invalid session");

            if (session.ExpiresAt < DateTime.UtcNow) return BadRequest("Code expired");

            if (session.Code != req.Code) return BadRequest("Invalid code");

            // mark verified
            session.Verified = true;
            await _db.SaveChangesAsync();

            var phone = session.Phone;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
            if (user == null)
            {
                user = new User { Id = Guid.NewGuid(), Phone = phone, Name = req.Name };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            var token = _tokenService.GenerateToken(user);
            return Ok(new AuthResponse(token, session.Id.ToString()));
        }
    }
}