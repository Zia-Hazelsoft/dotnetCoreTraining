namespace UserManagement.Api.Dtos
{
    public class CreateUserResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string ConfirmationLink { get; set; } = string.Empty;
    }
}
