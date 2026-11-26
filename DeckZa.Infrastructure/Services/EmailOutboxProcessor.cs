using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;

namespace DeckZa.Infrastructure.Services;

public class EmailOutboxProcessor
{
    private static readonly HttpClient HttpClient = new();
    private readonly IUnitOfWork _unitOfWork;
    private readonly IErrorLogService _errorLog;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public EmailOutboxProcessor(
        IUnitOfWork unitOfWork,
        IErrorLogService errorLog,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _errorLog = errorLog;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task ProcessPendingAsync()
    {
        var now = DateTime.UtcNow;
        var messages = await _unitOfWork.EmailOutbox.GetPendingAsync(now, 20);

        if (messages.Count == 0)
        {
            return;
        }

        var providers = await _unitOfWork.EmailProviders.GetActiveProvidersAsync();

        foreach (var message in messages)
        {
            var provider = providers.FirstOrDefault(p =>
                                string.Equals(p.provider_type, message.provider_type, StringComparison.OrdinalIgnoreCase))
                            ?? providers.FirstOrDefault();

            if (provider == null)
            {
                message.status = "AdminFallback";
                message.last_error = "No active email provider.";
                continue;
            }

            var attachmentPath = ResolveAttachmentPath(message, out var attachmentWarning);

            try
            {
                if (string.Equals(provider.provider_type, "SMTP", StringComparison.OrdinalIgnoreCase))
                {
                    await SendSmtpAsync(provider, message, GetPassPhrase(), attachmentPath);
                }
                else if (string.Equals(provider.provider_type, "GMAIL", StringComparison.OrdinalIgnoreCase))
                {
                    await SendGmailAsync(provider, message, GetPassPhrase(), attachmentPath);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported email provider.");
                }

                message.status = "Sent";
                message.sent_at = DateTime.UtcNow;
                message.last_error = attachmentWarning;
            }
            catch (Exception ex)
            {
                message.attempt_count++;
                message.status = message.attempt_count >= 3 ? "AdminFallback" : "Failed";
                message.next_attempt_at = DateTime.UtcNow.AddMinutes(Math.Pow(5, message.attempt_count));
                message.last_error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                await _errorLog.LogAsync("EmailOutbox", "ERROR", ex);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Menentukan path absolut lampiran. Bila file hilang, email tetap dikirim tanpa
    /// lampiran dan alasannya dicatat pada <c>last_error</c>.
    /// </summary>
    private string? ResolveAttachmentPath(tb_email_outbox message, out string? warning)
    {
        warning = null;

        if (string.IsNullOrWhiteSpace(message.attachment_storage_path))
        {
            return null;
        }

        var storedPath = message.attachment_storage_path!;
        var absolutePath = Path.IsPathRooted(storedPath)
            ? storedPath
            : Path.Combine(_environment.ContentRootPath, storedPath.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(absolutePath))
        {
            warning = $"Attachment file not found: {storedPath}. Email sent without attachment.";
            return null;
        }

        return absolutePath;
    }

    internal static async Task SendSmtpAsync(
        rf_email_provider provider,
        tb_email_outbox message,
        string? passPhrase = null,
        string? attachmentPath = null)
    {
        using var config = JsonDocument.Parse(provider.configuration_json ?? "{}");
        var root = config.RootElement;
        var host = GetString(root, "host");
        var port = GetInt(root, "port", 587);
        var username = GetString(root, "username");
        var password = SecretProtector.Unprotect(GetString(root, "password"), passPhrase);
        var fromEmail = provider.from_email ?? GetString(root, "fromEmail");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("SMTP host/from email is not configured.");
        }

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = GetBool(root, "enableSsl", true),
            Credentials = string.IsNullOrWhiteSpace(username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(username, password),
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(fromEmail, provider.from_name ?? "Personal Task and Report Management"),
            Subject = message.subject,
            Body = message.body_html ?? message.body_text ?? string.Empty,
            IsBodyHtml = !string.IsNullOrWhiteSpace(message.body_html),
        };
        mail.To.Add(message.recipient);

        if (!string.IsNullOrWhiteSpace(attachmentPath))
        {
            mail.Attachments.Add(new Attachment(attachmentPath));
        }

        await client.SendMailAsync(mail);
    }

    internal static async Task SendGmailAsync(
        rf_email_provider provider,
        tb_email_outbox message,
        string? passPhrase = null,
        string? attachmentPath = null)
    {
        using var config = JsonDocument.Parse(provider.configuration_json ?? "{}");
        var root = config.RootElement;
        var clientId = GetString(root, "clientId");
        var clientSecret = SecretProtector.Unprotect(GetString(root, "clientSecret"), passPhrase);
        var refreshToken = SecretProtector.Unprotect(GetString(root, "refreshToken"), passPhrase);
        var fromEmail = provider.from_email ?? GetString(root, "fromEmail");
        if (string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret)
            || string.IsNullOrWhiteSpace(refreshToken)
            || string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("Gmail OAuth configuration is incomplete.");
        }

        using var tokenResponse = await HttpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token",
            }));
        var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gmail token request failed: {tokenBody}");
        }

        using var tokenJson = JsonDocument.Parse(tokenBody);
        var accessToken = GetString(tokenJson.RootElement, "access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Gmail access token is empty.");
        }

        var rawMessage = BuildGmailRawMessage(fromEmail, message, attachmentPath);
        var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawMessage))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://gmail.googleapis.com/gmail/v1/users/me/messages/send");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new { raw });
        using var response = await HttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gmail send failed: {await response.Content.ReadAsStringAsync()}");
        }
    }

    /// <summary>
    /// Menyusun raw message MIME Gmail.
    /// Tanpa lampiran, format lama (text/html tunggal) dipertahankan agar perilaku
    /// email yang sudah berjalan tidak berubah.
    /// </summary>
    private static string BuildGmailRawMessage(string fromEmail, tb_email_outbox message, string? attachmentPath)
    {
        var body = message.body_html ?? message.body_text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(attachmentPath))
        {
            return $"From: {fromEmail}\r\n"
                   + $"To: {message.recipient}\r\n"
                   + $"Subject: {message.subject}\r\n"
                   + "MIME-Version: 1.0\r\n"
                   + "Content-Type: text/html; charset=UTF-8\r\n\r\n"
                   + body;
        }

        var boundary = $"deckza-{Guid.NewGuid():N}";
        var fileName = string.IsNullOrWhiteSpace(message.attachment_file_name)
            ? Path.GetFileName(attachmentPath)
            : message.attachment_file_name;
        var contentType = string.IsNullOrWhiteSpace(message.attachment_content_type)
            ? "application/octet-stream"
            : message.attachment_content_type;

        var bodyBase64 = ToBase64Lines(Encoding.UTF8.GetBytes(body));
        var fileBase64 = ToBase64Lines(File.ReadAllBytes(attachmentPath));

        return $"From: {fromEmail}\r\n"
               + $"To: {message.recipient}\r\n"
               + $"Subject: {message.subject}\r\n"
               + "MIME-Version: 1.0\r\n"
               + $"Content-Type: multipart/mixed; boundary=\"{boundary}\"\r\n\r\n"
               + $"--{boundary}\r\n"
               + "Content-Type: text/html; charset=UTF-8\r\n"
               + "Content-Transfer-Encoding: base64\r\n\r\n"
               + bodyBase64
               + $"--{boundary}\r\n"
               + $"Content-Type: {contentType}; name=\"{fileName}\"\r\n"
               + "Content-Transfer-Encoding: base64\r\n"
               + $"Content-Disposition: attachment; filename=\"{fileName}\"\r\n\r\n"
               + fileBase64
               + $"--{boundary}--";
    }

    /// <summary>Base64 dengan pemenggalan baris 76 karakter (aturan MIME).</summary>
    private static string ToBase64Lines(byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        var builder = new StringBuilder(base64.Length + (base64.Length / 76) * 2);

        for (var i = 0; i < base64.Length; i += 76)
        {
            builder.Append(base64, i, Math.Min(76, base64.Length - i)).Append("\r\n");
        }

        return builder.ToString();
    }

    private string? GetPassPhrase() => _configuration["Encryption:PassPhrase"];

    private static string? GetString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) ? value.GetString() : null;

    private static int GetInt(JsonElement element, string name, int fallback)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return fallback;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out number)
            ? number
            : fallback;
    }

    private static bool GetBool(JsonElement element, string name, bool fallback)
    {
        if (!element.TryGetProperty(name, out var value))
        {
            return fallback;
        }

        if (value.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            return value.GetBoolean();
        }

        return value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var result)
            ? result
            : fallback;
    }
}
