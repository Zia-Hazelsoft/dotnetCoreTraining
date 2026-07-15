namespace UserManagement.Api.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int ExpiryInMinutes { get; init; }
}