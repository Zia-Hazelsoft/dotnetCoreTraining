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
            IConfigurationSection jwtSettings = _config.GetSection("Jwt");
            string keyStr = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            double expiryMinutes = double.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");
            DateTime expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
