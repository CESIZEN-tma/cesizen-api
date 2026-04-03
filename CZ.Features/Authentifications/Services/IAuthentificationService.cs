using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;

namespace api.CZ.Features.Authentifications.Services;

public interface IAuthentificationService
{
    Task<Result<SimplyAuthResponse>> Login(LoginDto dto);
    Task<Result> ConfirmAccount(string token);
    Task<Result> RegisterUser(RegisterDto dto);
    Task<Result> ForgotPassword(ForgotPasswordDto dto);
    Task<Result> ResetPassword(ResetPasswordDto dto);
    Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto);
    Task<Result> Logout(string refreshToken);
    Task<Result<List<SessionDto>>> GetActiveSessions(Guid userId, string currentRefreshToken);
    Task<Result> RevokeSession(Guid userId, Guid sessionId);
    Task<Result> RevokeAllOtherSessions(Guid userId, string currentRefreshToken);
    Task<Result> ChangePassword(Guid userId, ChangePasswordDto dto, string currentRefreshToken);
}
