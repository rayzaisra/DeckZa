namespace DeckZa.Application.DTOs.Auth;

public class GoogleAuthConfigDto
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string Scope { get; set; } = "openid email profile";
}

public class GoogleSignInRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string CodeVerifier { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

public class GoogleSyncRequestDto
{
    public string? AccessToken { get; set; }
}
