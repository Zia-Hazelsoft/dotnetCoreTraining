using System;

namespace UserManagement.Api.Dtos
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expiration { get; set; }

        public UserDto User { get; set; } = null!;
    }
}
