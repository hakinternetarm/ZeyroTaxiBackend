using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Taxi_API.Data;
using Taxi_API.DTOs;
using Taxi_API.Models;
using Taxi_API.Services;

namespace Taxi_API.Controllers
{
    [ApiController]
    [Route("api/driver")]
    public class DriverAuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _email;

        public DriverAuthController(AppDbContext db, ITokenService tokenService, IEmailService email)
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

            await _email.SendAsync(req.Phone + "@example.com", "Your driver verification code", $"Your code is: {code}");

            // Do not create/register user here. Client must verify first.
            return Ok(new { Sent = true });
        }

        [HttpPost("resend")]
        public async Task<IActionResult> Resend([FromBody] ResendRequest req)
        {
            AuthSession? session = null;
            if (!string.IsNullOrWhiteSpace(req.AuthSessionId) && Guid.TryParse(req.AuthSessionId, out var sid))
            {
                session = await _db.AuthSessions.FirstOrDefaultAsync(s => s.Id == sid);
            }

            if (session == null && !string.IsNullOrWhiteSpace(req.Phone))
            {
                var code = new Random().Next(100000, 999999).ToString();
                session = new AuthSession
                {
                    Id = Guid.NewGuid(),
                    Phone = req.Phone!,
                    Code = code,
                    Verified = false,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };
                _db.AuthSessions.Add(session);
                await _db.SaveChangesAsync();
            }

            if (session == null) return BadRequest("No session or phone provided");

            session.Code = new Random().Next(100000, 999999).ToString();
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();
            await _email.SendAsync(session.Phone + "@example.com", "Your driver verification code (resend)", $"Your code is: {session.Code}");

            return Ok(new { Sent = true });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Phone) || string.IsNullOrWhiteSpace(req.Code)) return BadRequest("Phone and Code are required");

            var session = await _db.AuthSessions.OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync(s => s.Phone == req.Phone && s.Code == req.Code && s.ExpiresAt > DateTime.UtcNow);
            if (session == null) return BadRequest("Invalid or expired code");

            session.Verified = true;
            await _db.SaveChangesAsync();

            // create or fetch user and mark as driver
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
            if (user == null)
            {
                user = new User { Id = Guid.NewGuid(), Phone = req.Phone, Name = req.Name, IsDriver = true, PhoneVerified = true };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                user.IsDriver = true;
                user.PhoneVerified = true;
                if (!string.IsNullOrWhiteSpace(req.Name)) user.Name = req.Name;
                await _db.SaveChangesAsync();
            }

            return Ok(new { AuthSessionId = session.Id.ToString() });
        }

        [HttpPost("auth")]
        public async Task<IActionResult> Auth([FromBody] AuthRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.AuthSessionId) || string.IsNullOrWhiteSpace(req.Code))
                return BadRequest("AuthSessionId and Code are required");

            if (!Guid.TryParse(req.AuthSessionId, out var sessionId)) return BadRequest("Invalid AuthSessionId");

            var session = await _db.AuthSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null) return BadRequest("Invalid session");

            if (session.ExpiresAt < DateTime.UtcNow) return BadRequest("Code expired");

            if (session.Code != req.Code) return BadRequest("Invalid code");

            // require verification step completed first
            if (!session.Verified) return BadRequest("Session not verified");

            var phone = session.Phone;
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone && u.IsDriver);
            if (user == null) return BadRequest("Driver not registered or verified");

            var token = _tokenService.GenerateToken(user);
            return Ok(new AuthResponse(token, session.Id.ToString()));
        }

        [Authorize]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] object? body)
        {
            var userIdStr = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsDriver);
            if (user == null) return Unauthorized();
            var token = _tokenService.GenerateToken(user);
            return Ok(new AuthResponse(token, string.Empty));
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { LoggedOut = true });
        }
    }
}
