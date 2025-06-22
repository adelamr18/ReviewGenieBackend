namespace ReviewGenie.Infrastructure.Auth;

public class JwtSettings
{
    public string Secret { get; init; } = "";
    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public int ExpiryMinutes { get; init; } = 30;
}
