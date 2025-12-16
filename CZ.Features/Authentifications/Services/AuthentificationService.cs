using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.Users.Factories;
using api.CZ.Features.Users.Models;
using api.CZ.Features.Users.Repositories;
using Microsoft.AspNetCore.Mvc;
using Simply.Auth.AspNetCore.Models;
using Simply.Auth.Core.Abstractions;
using Simply.Auth.Core.Enums;

namespace api.CZ.Features.Authentifications.Services;

public class AuthentificationService : IAuthentificationService
{
    private readonly IUserRepository _userRepository;
    private readonly ISimplyAuthService _simplyAuthService;
    private readonly IUserFactory _userFactory;

    public AuthentificationService(IUserRepository userRepository, ISimplyAuthService simplyAuthService, IUserFactory userFactory)
    {
        _userRepository = userRepository;
        _simplyAuthService = simplyAuthService;
        _userFactory = userFactory;
    }

    public async Task<Result> RegisterUser(RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword) return Result.Failure("Password must be identical.");

        //Account with this email does not exist
        if (await _userRepository.AnyAsync(u => u.Email == dto.Email))
        {
            return Result.Failure("Email already exists");
        }

        var hash = _simplyAuthService.HashPassword(dto.Password);

        User newUserAccount = _userFactory.Create(dto.Email,dto.FirstName, dto.LastName, hash);
        newUserAccount.MemberSince = DateTime.Now;

        await _userRepository.AddAsync(newUserAccount);
        return Result.Success();
    }
    
    
    public async Task<Result<SimplyAuthResponse>> Login(LoginDto dto)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);
    
        if (user is null)
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");

        var result = _simplyAuthService.VerifyPassword(dto.Password, user.PasswordHash);

        if (result == SimplyVerificationResult.Failed)
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");

        if (result == SimplyVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _simplyAuthService.HashPassword(dto.Password);
            user.PasswordHash = newHash;
            await _userRepository.UpdateAsync(user);
        }

        var tokens = _simplyAuthService.GenerateTokens(user.Id.ToString());

        
        return Result.Success(new SimplyAuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration
        });

    }
}