using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;

namespace api.CZ.Features.Authentifications.Services;

public interface IAuthentificationService
{
    Task<Result<SimplyAuthResponse>> Login(LoginDto dto);
    Task<Result> RegisterUser(RegisterDto dto);
}
