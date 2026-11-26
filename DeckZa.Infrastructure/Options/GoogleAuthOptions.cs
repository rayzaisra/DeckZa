namespace DeckZa.Infrastructure.Options;

public class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string[] AllowedHosts { get; set; } = [];
}
