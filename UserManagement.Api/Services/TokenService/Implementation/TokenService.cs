using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Api.Configuration;
using UserManagement.Api.Models;
using UserManagement.Api.Services.TokenService;

namespace UserManagement.Api.Services.TokenService.Implementation
{
    /// <summary>
    /// Implements JSON Web Token (JWT) generation services for user sessions.
    /// </summary>
    public class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        /// <summary>
        /// Generates a signed, encrypted JWT security token containing claims for user profile data.
        /// </summary>
        /// <param name="user">The user context payload.</param>
        /// <returns>A signed string representation of the JWT token.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the JWT Secret Key is missing in the configuration.</exception>
        public string GenerateToken(User user)
        {
            string keyStr = _jwtSettings.Key ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(keyStr));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            ];

            DateTime expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
