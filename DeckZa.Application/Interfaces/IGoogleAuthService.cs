using DeckZa.Application.DTOs.Auth;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

public interface IGoogleAuthService
{
    GoogleAuthConfigDto GetConfig();
    bool IsEnabled { get; }
    Task<ApiResponse<LoginResponseDto>> SignInAsync(GoogleSignInRequestDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponseDto>> SyncProfileAsync(string userId, GoogleSyncRequestDto request, CancellationToken cancellationToken = default);
}
