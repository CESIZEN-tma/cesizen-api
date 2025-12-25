using api.CZ.Core.ResultPattern;
using api.CZ.Core.Services;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.EmailConfirmationTokens.Services;
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
    private readonly IEmailService _emailService;
    private readonly IEmailConfirmationTokenService _emailConfirmationTokenService;
    private readonly ILogger<AuthentificationService> _logger;

    public AuthentificationService(
        IUserRepository userRepository, 
        ISimplyAuthService simplyAuthService, 
        IUserFactory userFactory, 
        IEmailService emailService,
        IEmailConfirmationTokenService emailConfirmationTokenService,
        ILogger<AuthentificationService> logger)
    {
        _userRepository = userRepository;
        _simplyAuthService = simplyAuthService;
        _userFactory = userFactory;
        _emailService = emailService;
        _emailConfirmationTokenService = emailConfirmationTokenService;
        _logger = logger;
    }

    public async Task<Result> RegisterUser(RegisterDto dto)
    {
        _logger.LogInformation("Registration attempt for email {Email}", dto.Email);
        
        if (dto.Password != dto.ConfirmPassword)
        {
            _logger.LogWarning("Registration failed: password mismatch for {Email}", dto.Email);
            return Result.Failure("Password must be identical.");
        }

        if (await _userRepository.AnyAsync(u => u.Email == dto.Email))
        {
            _logger.LogWarning("Registration failed: email already exists {Email}", dto.Email);
            return Result.Failure("Email already exists");
        }

        var hash = _simplyAuthService.HashPassword(dto.Password);

        User newUserAccount = _userFactory.Create(dto.Email, dto.FirstName, dto.LastName, hash);
        newUserAccount.MemberSince = DateTime.Now;
        
        var newAccount = await _userRepository.AddAsync(newUserAccount);

        var confirmationToken = await _emailConfirmationTokenService.NewToken(newUserAccount.Id);
        
        await _emailService.SendRegisteringConfirmationEmail(
            confirmationToken.Token,
            newUserAccount.FirstName,
            newUserAccount.LastName,
            newUserAccount.Email, 
            "Confirmation de création de compte",
            "Confirmez votre compte");
            
        
        _logger.LogInformation("User registered successfully: {UserId}", newUserAccount.Id);
        
        return Result.Success();
    }
    
    public async Task<Result<SimplyAuthResponse>> Login(LoginDto dto)
    {
        _logger.LogInformation("Login attempt for email {Email}", dto.Email);
        
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);
    
        if (user is null)
        {
            _logger.LogWarning("Login failed: user not found for {Email}", dto.Email);
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        if (!user.AccountActivated)
        {
            _logger.LogWarning("Login failed: account not activated for {UserId}", user.Id);
            return Result.Failure<SimplyAuthResponse>("Le compte doit être activé.");
        }
        
        var result = _simplyAuthService.VerifyPassword(dto.Password, user.PasswordHash);

        if (result == SimplyVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed: invalid password for {UserId}", user.Id);
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        if (result == SimplyVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _simplyAuthService.HashPassword(dto.Password);
            user.PasswordHash = newHash;
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Password rehashed for user {UserId}", user.Id);
        }

        var tokens = _simplyAuthService.GenerateTokens(user.Id.ToString());

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);
        
        return Result.Success(new SimplyAuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration
        });
    }

    public async Task<Result> ConfirmAccount(string token)
    {
        _logger.LogInformation("Account confirmation attempt with token {Token}", token);

        var confirmationToken = await _emailConfirmationTokenService.GetEntityByToken(token);

        if (confirmationToken is null)
        {
            _logger.LogWarning("Account confirmation failed: token not found");
            return Result.Failure("Invalid token.");
        }

        if (confirmationToken.Consumed)
        {
            _logger.LogWarning("Account confirmation failed: token already consumed for user {UserId}", confirmationToken.IdUsers);
            return Result.Failure("Token already used.");
        }

        if (confirmationToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Account confirmation failed: token expired for user {UserId}", confirmationToken.IdUsers);
            return Result.Failure("Token expired.");
        }

        var user = await _userRepository.FindAsync(confirmationToken.IdUsers);

        if (user is null)
        {
            _logger.LogError("Account confirmation failed: user {UserId} not found for valid token", confirmationToken.IdUsers);
            return Result.Failure("User not found.");
        }

        if (user.AccountActivated)
        {
            _logger.LogWarning("Account confirmation attempted on already activated account {UserId}", user.Id);
            await _emailConfirmationTokenService.Consume(token);
            return Result.Success();
        }

        user.AccountActivated = true;
        user.UpdateTime = DateTime.UtcNow;
    
        await _userRepository.UpdateAsync(user);
        await _emailConfirmationTokenService.Consume(token);

        _logger.LogInformation("Account activated successfully for user {UserId}", user.Id);

        return Result.Success();
    }
}