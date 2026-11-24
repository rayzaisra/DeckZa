namespace DeckZa.Application.Common;

/// <summary>
/// Substitusi placeholder template email (rf_email_template).
/// Dipakai bersama oleh alur pendaftaran (AuthService) dan alur approval
/// (ApprovalService) agar format penulisan template konsisten di satu tempat.
/// </summary>
public static class EmailTemplateRenderer
{
    public static string ApplyValues(
        string value,
        string fullName,
        string userId,
        string? shortText,
        string? longText,
        string? link)
        => value
            .Replace("{{full_name}}", fullName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{user_id}}", userId, StringComparison.OrdinalIgnoreCase)
            .Replace("{{short_text}}", shortText ?? string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("{{long_text}}", longText ?? string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("{{link}}", link ?? string.Empty, StringComparison.OrdinalIgnoreCase);
}
