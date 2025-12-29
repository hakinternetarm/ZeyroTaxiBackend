using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Taxi_API.Models;

namespace Taxi_API.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var key = _config["Jwt:Key"] ?? "very_secret_key_please_change";
            var issuer = _config["Jwt:Issuer"] ?? "TaxiApi";

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("phone", user.Phone),
                new Claim("isDriver", user.IsDriver.ToString())
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer, issuer, claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}