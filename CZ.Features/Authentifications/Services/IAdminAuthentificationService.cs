using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;

namespace api.CZ.Features.Authentifications.Services;

public interface IAdminAuthentificationService
{
    Task<Result<SimplyAuthResponse>> Login(LoginDto dto);
    Task<Result> ConfirmAccount(string token);
    Task<Result> RegisterAdmin(RegisterDto dto);
    Task<Result> ForgotPassword(ForgotPasswordDto dto);
    Task<Result> ResetPassword(ResetPasswordDto dto);
    Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto);
    Task<Result> Logout(string refreshToken);
}
