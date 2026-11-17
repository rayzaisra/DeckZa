using DeckZa.Application.DTOs.Auth;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

public interface IPasswordResetService
{
    Task<PasswordResetRequestResult> RequestAsync(string userIdOrEmail, string? ipAddress, string? publicBaseUrl = null);
    Task<bool> ValidateTokenAsync(string token);
    Task<BaseGeneralResponse> ResetAsync(string token, string newPassword, string confirmNewPassword);
}
