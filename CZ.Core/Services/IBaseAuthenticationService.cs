using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;

namespace api.CZ.Core.Services;

/// <summary>
/// Base interface for authentication services supporting different entity types (Users, Administrators).
/// Provides common authentication operations including registration, login, password management, and token refresh.
/// </summary>
public interface IBaseAuthenticationService
{
    /// <summary>
    /// Authenticates an entity and returns access and refresh tokens
    /// </summary>
    Task<Result<SimplyAuthResponse>> Login(LoginDto dto);

    /// <summary>
    /// Confirms an entity's account using an email confirmation token
    /// </summary>
    Task<Result> ConfirmAccount(string token);

    /// <summary>
    /// Initiates the password reset process by sending a reset email
    /// </summary>
    Task<Result> ForgotPassword(ForgotPasswordDto dto);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token
    /// </summary>
    Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto);

    /// <summary>
    /// Logs out an entity by consuming their refresh token
    /// </summary>
    Task<Result> Logout(string refreshToken);
}
