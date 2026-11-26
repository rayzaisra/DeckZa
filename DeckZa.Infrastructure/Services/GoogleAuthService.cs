using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Google.Apis.Auth;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using DeckZa.Application.Common;
using DeckZa.Application.DTOs.Auth;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;
using DeckZa.Infrastructure.Options;

namespace DeckZa.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private const string Provider = "GOOGLE";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IJWTService _jwtService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserSessionNotifier _sessionNotifier;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IErrorLogService _errorLog;
    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly IConfiguration _configuration;

    public GoogleAuthService(
        IUnitOfWork unitOfWork,
        IJWTService jwtService,
        ICurrentUserService currentUser,
        IUserSessionNotifier sessionNotifier,
        IHttpClientFactory httpClientFactory,
        IErrorLogService errorLog,
        IOptions<GoogleAuthOptions> options,
        ILogger<GoogleAuthService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _currentUser = currentUser;
        _sessionNotifier = sessionNotifier;
        _httpClientFactory = httpClientFactory;
        _errorLog = errorLog;
        _options = options.Value;
        _logger = logger;
        _configuration = configuration;
    }

    public bool IsEnabled => _options.Enabled
        && !string.IsNullOrWhiteSpace(_options.ClientId)
        && !string.IsNullOrWhiteSpace(_options.ClientSecret);

    public GoogleAuthConfigDto GetConfig()
    {
        if (!IsEnabled)
        {
            return new GoogleAuthConfigDto { Enabled = false };
        }

        return new GoogleAuthConfigDto
        {
            Enabled = true,
            ClientId = _options.ClientId,
            RedirectUri = _options.RedirectUri ?? string.Empty,
            Scope = "openid email profile",
        };
    }

    public async Task<ApiResponse<LoginResponseDto>> SignInAsync(
        GoogleSignInRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return NotFound<LoginResponseDto>();
        }

        if (string.IsNullOrWhiteSpace(request.Code)
            || string.IsNullOrWhiteSpace(request.CodeVerifier)
            || string.IsNullOrWhiteSpace(request.RedirectUri))
        {
            return Fail<LoginResponseDto>("Invalid Google sign-in request.");
        }

        if (!IsAllowedRedirectUri(request.RedirectUri))
        {
            await _errorLog.LogAsync("Auth", "WARN",
                new InvalidOperationException("Google sign-in rejected: redirect URI host is not allowed."));
            return Fail<LoginResponseDto>("Redirect URI is not allowed.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var tokenResponse = await ExchangeCodeAsync(request, cancellationToken);
            if (string.IsNullOrWhiteSpace(tokenResponse.IdToken))
            {
                return Fail<LoginResponseDto>("Google did not return a valid identity token.");
            }

            payload = await GoogleJsonWebSignature.ValidateAsync(
                tokenResponse.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_options.ClientId],
                });
        }
        catch (InvalidJwtException ex)
        {
            await _errorLog.LogAsync("Auth", "WARN", ex);
            return Fail<LoginResponseDto>("Google identity token is invalid.");
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return Fail<LoginResponseDto>("Unable to sign in with Google. Please try again.");
        }

        if (!IsTrustedIssuer(payload.Issuer))
        {
            return Fail<LoginResponseDto>("Google identity token issuer is invalid.");
        }

        if (payload.EmailVerified != true)
        {
            return Fail<LoginResponseDto>("Google email is not verified.");
        }

        if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
        {
            return Fail<LoginResponseDto>("Google account information is incomplete.");
        }

        try
        {
            var login = await ResolveOrCreateUserAsync(payload, cancellationToken);
            return Ok(login, "Signed in with Google.");
        }
        catch (GoogleAuthConflictException ex)
        {
            return Conflict<LoginResponseDto>(ex.Message);
        }
        catch (GoogleAuthForbiddenException ex)
        {
            return Fail<LoginResponseDto>(ex.Message, Const.ERROR_CODE);
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return Fail<LoginResponseDto>("Unable to sign in with Google. Please try again.");
        }
    }

    public async Task<ApiResponse<UserResponseDto>> SyncProfileAsync(
        string userId,
        GoogleSyncRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!IsEnabled)
        {
            return NotFound<UserResponseDto>();
        }

        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return Fail<UserResponseDto>("Google access token is required.");
        }

        var user = await _unitOfWork.UmUserApp.GetByIdAsync(userId);
        if (user == null || user.isDeleted == true || user.isActive != true)
        {
            return NotFound<UserResponseDto>("User not found.");
        }

        if (!string.Equals(user.external_provider, Provider, StringComparison.OrdinalIgnoreCase))
        {
            return Fail<UserResponseDto>("This account is not linked to Google.");
        }

        GoogleUserInfo userInfo;
        try
        {
            userInfo = await GetUserInfoAsync(request.AccessToken, cancellationToken);
        }
        catch (GoogleAuthUnauthorizedException)
        {
            return Unauthorized<UserResponseDto>("Google access token expired. Please sign in with Google again.");
        }
        catch (Exception ex)
        {
            await _errorLog.LogAsync("Auth", "ERROR", ex);
            return Fail<UserResponseDto>("Unable to sync profile from Google.");
        }

        if (string.IsNullOrWhiteSpace(userInfo.Sub) || string.IsNullOrWhiteSpace(userInfo.Email))
        {
            return Fail<UserResponseDto>("Google profile information is incomplete.");
        }

        if (!string.Equals(userInfo.Sub, user.external_provider_id, StringComparison.Ordinal))
        {
            return Fail<UserResponseDto>("Google account does not match this user.");
        }

        if (userInfo.EmailVerified != true)
        {
            return Fail<UserResponseDto>("Google email is not verified.");
        }

        var conflicting = await _unitOfWork.UmUserApp.GetByEmailAsync(userInfo.Email, cancellationToken);
        if (conflicting != null
            && !string.Equals(conflicting.user_id, user.user_id, StringComparison.OrdinalIgnoreCase))
        {
            return Conflict<UserResponseDto>("Email ini sudah terdaftar. Gunakan akun Google lain.");
        }

        user.full_name = string.IsNullOrWhiteSpace(userInfo.Name) ? user.full_name : userInfo.Name;
        user.email = userInfo.Email;
        user.avatar_url = string.IsNullOrWhiteSpace(userInfo.Picture) ? user.avatar_url : userInfo.Picture;
        user.profile_synced_date = DateTime.Now;
        user.modifiedDate = DateTime.Now;
        user.modifiedBy = user.user_id;

        await _unitOfWork.UmUserApp.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mapped = await MapToUserDtoAsync(user, cancellationToken);
        return Ok(mapped, "Profile synced from Google.");
    }

    private async Task<LoginResponseDto> ResolveOrCreateUserAsync(
        GoogleJsonWebSignature.Payload payload,
        CancellationToken cancellationToken)
    {
        var byProvider = await _unitOfWork.UmUserApp.GetByExternalProviderIdAsync(Provider, payload.Subject, cancellationToken);
        if (byProvider != null)
        {
            return await LoginExistingUserAsync(byProvider, cancellationToken);
        }

        var byEmail = await _unitOfWork.UmUserApp.GetByEmailAsync(payload.Email, cancellationToken);
        if (byEmail != null)
        {
            if (string.IsNullOrWhiteSpace(byEmail.external_provider_id))
            {
                throw new GoogleAuthConflictException(
                    "Email ini sudah terdaftar dengan akun form. Silakan masuk menggunakan username/password atau gunakan Lupa Password.");
            }

            if (!string.Equals(byEmail.external_provider, Provider, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(byEmail.external_provider_id, payload.Subject, StringComparison.Ordinal))
            {
                throw new GoogleAuthConflictException(
                    "Email ini sudah terdaftar dengan metode masuk lain.");
            }

            return await LoginExistingUserAsync(byEmail, cancellationToken);
        }

        var created = await CreateGoogleUserAsync(payload, cancellationToken);
        return await IssueLoginAsync(created, cancellationToken, previousSessionId: null);
    }

    private async Task<LoginResponseDto> LoginExistingUserAsync(um_user_app user, CancellationToken cancellationToken)
    {
        if (user.isDeleted == true || user.isActive != true)
        {
            throw new GoogleAuthForbiddenException("Account is not active. Please contact administrator.");
        }

        var flag = await EnsureFlagAsync(user.user_id, cancellationToken);
        if (flag.is_locked == true)
        {
            throw new GoogleAuthForbiddenException("Account is locked. Please contact administrator.");
        }

        return await IssueLoginAsync(user, cancellationToken, flag.session_id, flag);
    }

    private async Task<um_user_app> CreateGoogleUserAsync(
        GoogleJsonWebSignature.Payload payload,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var userId = await GenerateUniqueUserIdAsync(payload.Email, cancellationToken);

        var user = new um_user_app
        {
            user_id = userId,
            full_name = string.IsNullOrWhiteSpace(payload.Name) ? userId : payload.Name,
            email = payload.Email,
            avatar_url = payload.Picture,
            password = null,
            external_provider = Provider,
            external_provider_id = payload.Subject,
            registration_source = Const.RegistrationSource.SELF,
            plan_id = Const.PlanIds.Guest,
            statusApproval = Const.StatusApproval.APPROVED,
            isActive = true,
            isDeleted = false,
            profile_synced_date = now,
            createdBy = userId,
            createdDate = now,
        };

        await _unitOfWork.UmUserApp.AddAsync(user, cancellationToken);
        await _unitOfWork.UmUserGroup.AddAsync(new um_user_group
        {
            user_id = userId,
            group_id = Const.DEFAULT_GROUP_ID_GUEST_CODE,
            can_add_edit = true,
            can_delete = true,
            can_read_only = false,
            statusApproval = Const.StatusApproval.APPROVED,
            isActive = true,
            isDeleted = false,
            createdBy = userId,
            createdDate = now,
        }, cancellationToken);

        await _unitOfWork.UmUserAppFlag.AddAsync(new um_user_app_flag
        {
            user_id = userId,
            is_first_login = false,
            is_locked = false,
            is_login = false,
            failed_password_attempt_count = 0,
            ip_address = _currentUser.IpAddress,
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return user;
    }

    private async Task<LoginResponseDto> IssueLoginAsync(
        um_user_app user,
        CancellationToken cancellationToken,
        string? previousSessionId,
        um_user_app_flag? flag = null)
    {
        flag ??= await EnsureFlagAsync(user.user_id, cancellationToken);

        var userGroup = await _unitOfWork.UmUserGroup.GetByIdAsync(user.user_id);
        var role = userGroup?.group_id;
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new GoogleAuthForbiddenException("User has no assigned role. Please contact administrator.");
        }

        var sessionId = Guid.NewGuid().ToString("N");
        previousSessionId ??= flag.session_id;

        flag.failed_password_attempt_count = 0;
        flag.failed_password_attempt_date = null;
        flag.last_login_date = DateTime.Now;
        flag.is_login = true;
        flag.ip_address = _currentUser.IpAddress;
        flag.session_id = sessionId;

        await _unitOfWork.UmUserAppFlag.UpdateAsync(flag);
        await _unitOfWork.UmUserAppSession.UpsertAsync(user.user_id, sessionId);
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.user_id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.Equals(previousSessionId, sessionId, StringComparison.Ordinal))
        {
            await _sessionNotifier.NotifySessionRevokedAsync(user.user_id, previousSessionId);
        }

        var accessToken = _jwtService.GenerateAccessToken(user, [role], sessionId);
        var rawRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddHours(GetConfigInt("Jwt:SessionRefreshTokenHours", 12));

        var refreshEntity = new um_user_refresh_token
        {
            user_id = user.user_id,
            refresh_token = HashRefreshToken(rawRefreshToken),
            expiry_date = refreshExpiry,
            is_revoked = false,
            remember_me = false,
            createdDate = DateTime.Now,
            isActive = true,
            isDeleted = false,
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mapped = await MapToUserDtoAsync(user, cancellationToken);
        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(GetConfigInt("Jwt:AccessTokenExpirationMinutes", 15)),
            RefreshTokenExpiry = refreshEntity.expiry_date,
            User = mapped,
            UrlDestination = "login",
        };
    }

    private async Task<um_user_app_flag> EnsureFlagAsync(string userId, CancellationToken cancellationToken)
    {
        var flag = await _unitOfWork.UmUserAppFlag.GetByUserIdAsync(userId);
        if (flag != null)
        {
            return flag;
        }

        flag = new um_user_app_flag
        {
            user_id = userId,
            is_first_login = false,
            is_locked = false,
            is_login = false,
            failed_password_attempt_count = 0,
            ip_address = _currentUser.IpAddress,
            last_login_date = DateTime.Now,
        };
        await _unitOfWork.UmUserAppFlag.AddAsync(flag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return flag;
    }

    private async Task<UserResponseDto> MapToUserDtoAsync(um_user_app user, CancellationToken cancellationToken)
    {
        var userGroup = await _unitOfWork.UmUserGroup.GetByIdAsync(user.user_id);
        var flag = await _unitOfWork.UmUserAppFlag.GetByUserIdAsync(user.user_id);
        var roleName = userGroup?.group_id == null
            ? null
            : await _unitOfWork.UmUserGroup.GetGroupNameAsync(userGroup.group_id);
        var isSuperAdmin = string.Equals(userGroup?.group_id, "00", StringComparison.Ordinal);

        return new UserResponseDto
        {
            UserId = user.user_id,
            UserName = user.user_id,
            FullName = user.full_name,
            RoleId = userGroup?.group_id,
            RoleName = roleName,
            Email = user.email,
            PhoneNumber = user.phone_number,
            GenderId = user.gender_id,
            ExternalProvider = user.external_provider,
            AvatarUrl = user.avatar_url,
            ProfileSyncedDate = user.profile_synced_date,
            LastLoginDate = flag?.last_login_date ?? DateTime.Now,
            LastPasswordChangeDate = user.last_password_change_date,
            LastIpAddress = flag?.ip_address,
            CanAddEdit = isSuperAdmin || userGroup?.can_add_edit == true,
            CanDelete = isSuperAdmin || userGroup?.can_delete == true,
            CanReadOnly = !isSuperAdmin && userGroup?.can_read_only == true,
            IsActive = user.isActive == true,
        };
    }

    private async Task<GoogleTokenResponse> ExchangeCodeAsync(GoogleSignInRequestDto request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = request.Code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = request.RedirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = request.CodeVerifier,
        });

        using var response = await client.PostAsync(TokenEndpoint, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Google token exchange failed: {Status} {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException("Google token exchange failed.");
        }

        var token = JsonSerializer.Deserialize<GoogleTokenResponse>(body, JsonOptions);
        return token ?? throw new InvalidOperationException("Google token exchange returned an invalid response.");
    }

    private async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await client.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new GoogleAuthUnauthorizedException();
        }

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GoogleUserInfo>(body, JsonOptions)
            ?? throw new InvalidOperationException("Google user info response is invalid.");
    }

    private async Task<string> GenerateUniqueUserIdAsync(string email, CancellationToken cancellationToken)
    {
        var localPart = email.Split('@')[0].Trim().ToLowerInvariant();
        var sanitized = Regex.Replace(localPart, "[^a-z0-9._-]", string.Empty);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            sanitized = "googleuser";
        }

        if (sanitized.Length > 90)
        {
            sanitized = sanitized[..90];
        }

        var baseId = sanitized;
        var candidate = baseId;
        var suffix = 2;
        while (await _unitOfWork.UmUserApp.ExistsAnyAsync(candidate, cancellationToken))
        {
            var tail = suffix.ToString();
            var maxBaseLength = Math.Max(1, 100 - tail.Length);
            candidate = baseId.Length > maxBaseLength ? baseId[..maxBaseLength] + tail : baseId + tail;
            suffix++;
        }

        return candidate;
    }

    private bool IsAllowedRedirectUri(string redirectUri)
    {
        if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        if (_options.AllowedHosts.Length == 0)
        {
            return false;
        }

        return _options.AllowedHosts.Any(host =>
            string.Equals(host, uri.Host, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsTrustedIssuer(string? issuer)
        => string.Equals(issuer, "accounts.google.com", StringComparison.OrdinalIgnoreCase)
            || string.Equals(issuer, "https://accounts.google.com", StringComparison.OrdinalIgnoreCase);

    private int GetConfigInt(string key, int defaultValue)
    {
        return int.TryParse(_configuration[key], out var value) ? value : defaultValue;
    }

    private static string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    private static ApiResponse<T> Ok<T>(T data, string message)
        => new() { Success = true, Code = Const.SUCCESS_CODE, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message, string code = Const.ERROR_CODE)
        => new() { Success = false, Code = code, Message = message };

    private static ApiResponse<T> Conflict<T>(string message)
        => new() { Success = false, Code = "409", Message = message };

    private static ApiResponse<T> Unauthorized<T>(string message)
        => new() { Success = false, Code = "401", Message = message };

    private static ApiResponse<T> NotFound<T>(string? message = null)
        => new() { Success = false, Code = Const.DATA_NOT_FOUND_CODE, Message = message ?? "Not found." };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed class GoogleTokenResponse
    {
        public string? access_token { get; set; }
        public string? IdToken => id_token;
        public string? id_token { get; set; }
    }

    private sealed class GoogleUserInfo
    {
        public string? sub { get; set; }
        public string? email { get; set; }
        public bool email_verified { get; set; }
        public string? name { get; set; }
        public string? picture { get; set; }

        public string? Sub => sub;
        public string? Email => email;
        public bool? EmailVerified => email_verified;
        public string? Name => name;
        public string? Picture => picture;
    }

    private sealed class GoogleAuthConflictException(string message) : Exception(message);
    private sealed class GoogleAuthForbiddenException(string message) : Exception(message);
    private sealed class GoogleAuthUnauthorizedException : Exception;
}
