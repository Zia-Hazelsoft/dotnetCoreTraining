using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using UserManagement.Api.Models;
using UserManagement.Api.Services.Interfaces;

namespace UserManagement.Api.Services
{
    /// <summary>
    /// Implements JSON Web Token (JWT) generation services for user sessions.
    /// </summary>
    public class TokenService(IConfiguration config) : ITokenService
    {
        private readonly IConfiguration _config = config;

        /// <summary>
        /// Generates a signed, encrypted JWT security token containing claims for user profile data.
        /// </summary>
        /// <param name="user">The user context payload.</param>
        /// <returns>A signed string representation of the JWT token.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the JWT Secret Key is missing in the configuration.</exception>
        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var keyStr = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            var expiryMinutes = double.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");
            var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
