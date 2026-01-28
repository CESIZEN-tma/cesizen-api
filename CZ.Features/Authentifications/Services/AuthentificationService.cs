using api.CZ.Core.ResultPattern;
using api.CZ.Core.Services;
using api.CZ.Features.Authentifications.DTOs;
using api.CZ.Features.EmailConfirmationTokens.Services;
using api.CZ.Features.PasswordHistories.Services;
using api.CZ.Features.PasswordResetTokens.Services;
using api.CZ.Features.Sessions.Services;
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
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutDurationMinutes = 15;

    private readonly IUserRepository _userRepository;
    private readonly ISimplyAuthService _simplyAuthService;
    private readonly IUserFactory _userFactory;
    private readonly IEmailService _emailService;
    private readonly IEmailConfirmationTokenService _emailConfirmationTokenService;
    private readonly IPasswordResetTokenService _passwordResetTokenService;
    private readonly ISessionService _sessionService;
    private readonly IPasswordHistoryManager _passwordHistoryManager;
    private readonly ILogger<AuthentificationService> _logger;

    public AuthentificationService(
        IUserRepository userRepository,
        ISimplyAuthService simplyAuthService,
        IUserFactory userFactory,
        IEmailService emailService,
        IEmailConfirmationTokenService emailConfirmationTokenService,
        IPasswordResetTokenService passwordResetTokenService,
        ISessionService sessionService,
        IPasswordHistoryManager passwordHistoryManager,
        ILogger<AuthentificationService> logger)
    {
        _userRepository = userRepository;
        _simplyAuthService = simplyAuthService;
        _userFactory = userFactory;
        _emailService = emailService;
        _emailConfirmationTokenService = emailConfirmationTokenService;
        _passwordResetTokenService = passwordResetTokenService;
        _sessionService = sessionService;
        _passwordHistoryManager = passwordHistoryManager;
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

        // Check if account is locked
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
            _logger.LogWarning("Login failed: account locked for {UserId}, {Minutes} minutes remaining", user.Id, remainingMinutes);
            return Result.Failure<SimplyAuthResponse>($"Account is locked. Please try again in {remainingMinutes} minute(s).");
        }

        if (!user.AccountActivated)
        {
            _logger.LogWarning("Login failed: account not activated for {UserId}", user.Id);
            return Result.Failure<SimplyAuthResponse>("Le compte doit être activé.");
        }

        if (!user.Active)
        {
            _logger.LogWarning("Login failed: account disabled for {UserId}", user.Id);
            return Result.Failure<SimplyAuthResponse>("Account has been disabled. Please contact support.");
        }

        var result = _simplyAuthService.VerifyPassword(dto.Password, user.PasswordHash);

        if (result == SimplyVerificationResult.Failed)
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;
            user.UpdateTime = DateTime.UtcNow;

            if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                _logger.LogWarning("Account locked for {UserId} after {Attempts} failed attempts", user.Id, user.FailedLoginAttempts);
            }

            await _userRepository.UpdateAsync(user);
            _logger.LogWarning("Login failed: invalid password for {UserId}, attempt {Attempt}", user.Id, user.FailedLoginAttempts);
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        // Reset failed login attempts on successful login
        if (user.FailedLoginAttempts > 0 || user.LockedUntil.HasValue)
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.UpdateTime = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        if (result == SimplyVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _simplyAuthService.HashPassword(dto.Password);
            user.PasswordHash = newHash;
            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Password rehashed for user {UserId}", user.Id);
        }

        var tokens = _simplyAuthService.GenerateTokens(user.Id.ToString());

        // Create session for refresh token
        await _sessionService.CreateSession(
            user.Id,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiration);

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

    public async Task<Result> ForgotPassword(ForgotPasswordDto dto)
    {
        _logger.LogInformation("Password reset request for email {Email}", dto.Email);

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // SECURITY: Don't reveal if email exists or not (prevents email enumeration)
        // Always return success, but only send email if user exists
        if (user is null)
        {
            _logger.LogWarning("Password reset requested for non-existent email {Email}", dto.Email);
            // Simulate processing time to prevent timing attacks
            await Task.Delay(Random.Shared.Next(100, 300));
            return Result.Success();
        }

        if (!user.AccountActivated)
        {
            _logger.LogWarning("Password reset requested for unactivated account {UserId}", user.Id);
            // Still return success to prevent email enumeration
            return Result.Success();
        }

        // Generate and store reset token
        var resetToken = await _passwordResetTokenService.NewToken(user.Id);

        // Send password reset email
        var emailResult = await _emailService.SendPasswordResetEmail(
            resetToken.Token,
            user.FirstName,
            user.LastName,
            user.Email,
            "Réinitialisation de votre mot de passe",
            "Vous avez demandé à réinitialiser votre mot de passe.",
            TimeSpan.FromMinutes(15));

        if (!emailResult.IsSuccess)
        {
            _logger.LogError("Failed to send password reset email to {Email}: {Error}",
                user.Email, emailResult.Error);
            return Result.Failure("Failed to send reset email. Please try again later.");
        }

        _logger.LogInformation("Password reset email sent successfully to {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> ResetPassword(ResetPasswordDto dto)
    {
        _logger.LogInformation("Password reset attempt with token");

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            _logger.LogWarning("Password reset failed: password mismatch");
            return Result.Failure("Passwords must match.");
        }

        var resetToken = await _passwordResetTokenService.GetEntityByToken(dto.Token);

        if (resetToken is null)
        {
            _logger.LogWarning("Password reset failed: invalid or expired token");
            return Result.Failure("Invalid or expired reset token.");
        }

        if (resetToken.Consumed)
        {
            _logger.LogWarning("Password reset failed: token already used for user {UserId}",
                resetToken.IdUsers);
            return Result.Failure("This reset link has already been used.");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset failed: token expired for user {UserId}",
                resetToken.IdUsers);
            return Result.Failure("This reset link has expired. Please request a new one.");
        }

        var user = await _userRepository.FindAsync(resetToken.IdUsers);

        if (user is null)
        {
            _logger.LogError("Password reset failed: user {UserId} not found for valid token",
                resetToken.IdUsers);
            return Result.Failure("User not found.");
        }

        // Ensure password info exists
        await _passwordHistoryManager.EnsurePasswordsInfoExistsAsync(user.Id);

        // Check if new password is one of the last 5 passwords
        var isPasswordReused = await _passwordHistoryManager.IsPasswordReusedAsync(user.Id, dto.NewPassword);
        if (isPasswordReused)
        {
            _logger.LogWarning("Password reset failed: user {UserId} attempted to reuse a recent password", user.Id);
            return Result.Failure("You cannot reuse any of your last 5 passwords. Please choose a different password.");
        }

        // Add current password to history before resetting
        await _passwordHistoryManager.AddPasswordToHistoryAsync(user.Id, user.PasswordHash);

        // Hash the new password
        var newHash = _simplyAuthService.HashPassword(dto.NewPassword);

        // Update user's password
        user.PasswordHash = newHash;
        user.UpdateTime = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Consume the reset token
        await _passwordResetTokenService.Consume(dto.Token);

        // Send confirmation email
        await _emailService.SendPasswordResetConfirmationEmail(
            user.FirstName,
            user.LastName,
            user.Email,
            "Votre mot de passe a été modifié",
            "Votre mot de passe a été modifié avec succès.");

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);

        return Result.Success();
    }

    public async Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto)
    {
        _logger.LogInformation("Refresh token attempt");

        // Get session by refresh token
        var session = await _sessionService.GetByRefreshToken(dto.RefreshToken);

        if (session is null)
        {
            _logger.LogWarning("Refresh token attempt with invalid or expired token");
            return Result.Failure<SimplyAuthResponse>("Invalid or expired refresh token.");
        }

        // Get user
        var user = await _userRepository.FindAsync(session.IdUsers);

        if (user is null)
        {
            _logger.LogError("User {UserId} not found for valid session", session.IdUsers);
            return Result.Failure<SimplyAuthResponse>("User not found.");
        }

        if (!user.AccountActivated)
        {
            _logger.LogWarning("Refresh token attempt for unactivated account {UserId}", user.Id);
            return Result.Failure<SimplyAuthResponse>("Account is not activated.");
        }

        // Consume old session
        await _sessionService.ConsumeSession(dto.RefreshToken);

        // Generate new tokens
        var tokens = _simplyAuthService.GenerateTokens(user.Id.ToString());

        // Create new session for new refresh token
        await _sessionService.CreateSession(
            user.Id,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiration);

        _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

        return Result.Success(new SimplyAuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration
        });
    }

    public async Task<Result> Logout(string refreshToken)
    {
        _logger.LogInformation("Logout attempt");

        var consumed = await _sessionService.ConsumeSession(refreshToken);

        if (!consumed)
        {
            _logger.LogWarning("Logout attempted with invalid or already consumed refresh token");
            // Still return success for security (don't reveal session state)
            return Result.Success();
        }

        _logger.LogInformation("User logged out successfully");

        return Result.Success();
    }

    public async Task<Result<List<SessionDto>>> GetActiveSessions(Guid userId, string currentRefreshToken)
    {
        _logger.LogInformation("Retrieving active sessions for user {UserId}", userId);

        var sessions = await _sessionService.GetActiveSessionsByUserId(userId);
        var currentSession = await _sessionService.GetByRefreshToken(currentRefreshToken);

        var sessionDtos = sessions.Select(s => new SessionDto
        {
            Id = s.Id,
            CreatedAt = s.CreationTime,
            ExpiresAt = s.ExpiresAt,
            IsCurrentSession = currentSession != null && s.Id == currentSession.Id
        }).ToList();

        return Result.Success(sessionDtos);
    }

    public async Task<Result> RevokeSession(Guid userId, Guid sessionId)
    {
        _logger.LogInformation("Revoking session {SessionId} for user {UserId}", sessionId, userId);

        var revoked = await _sessionService.RevokeSessionForUser(sessionId, userId);

        if (!revoked)
        {
            _logger.LogWarning("Session {SessionId} not found for user {UserId}", sessionId, userId);
            return Result.Failure("Session not found.");
        }

        return Result.Success();
    }

    public async Task<Result> RevokeAllOtherSessions(Guid userId, string currentRefreshToken)
    {
        _logger.LogInformation("Revoking all other sessions for user {UserId}", userId);

        var currentSession = await _sessionService.GetByRefreshToken(currentRefreshToken);

        if (currentSession == null)
        {
            _logger.LogWarning("Current session not found for user {UserId}", userId);
            return Result.Failure("Current session not found.");
        }

        await _sessionService.RevokeAllSessionsExceptCurrent(userId, currentSession.Id);

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid userId, ChangePasswordDto dto, string currentRefreshToken)
    {
        _logger.LogInformation("Password change attempt for user {UserId}", userId);

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            _logger.LogWarning("Password change failed: passwords don't match for user {UserId}", userId);
            return Result.Failure("New passwords must match.");
        }

        var user = await _userRepository.FindAsync(userId);

        if (user == null)
        {
            _logger.LogError("Password change failed: user {UserId} not found", userId);
            return Result.Failure("User not found.");
        }

        var verifyResult = _simplyAuthService.VerifyPassword(dto.CurrentPassword, user.PasswordHash);

        if (verifyResult == Simply.Auth.Core.Enums.SimplyVerificationResult.Failed)
        {
            _logger.LogWarning("Password change failed: current password incorrect for user {UserId}", userId);
            return Result.Failure("Current password is incorrect.");
        }

        // Ensure password info exists
        await _passwordHistoryManager.EnsurePasswordsInfoExistsAsync(userId);

        // Check if new password is one of the last 5 passwords
        var isPasswordReused = await _passwordHistoryManager.IsPasswordReusedAsync(userId, dto.NewPassword);
        if (isPasswordReused)
        {
            _logger.LogWarning("Password change failed: user {UserId} attempted to reuse a recent password", userId);
            return Result.Failure("You cannot reuse any of your last 5 passwords. Please choose a different password.");
        }

        // Add current password to history before changing
        await _passwordHistoryManager.AddPasswordToHistoryAsync(userId, user.PasswordHash);

        // Hash the new password
        var newHash = _simplyAuthService.HashPassword(dto.NewPassword);

        user.PasswordHash = newHash;
        user.UpdateTime = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        // Revoke all other sessions for security
        var currentSession = await _sessionService.GetByRefreshToken(currentRefreshToken);
        if (currentSession != null)
        {
            await _sessionService.RevokeAllSessionsExceptCurrent(userId, currentSession.Id);
        }

        // Send confirmation email
        await _emailService.SendPasswordResetConfirmationEmail(
            user.FirstName,
            user.LastName,
            user.Email,
            "Votre mot de passe a été modifié",
            "Votre mot de passe a été modifié avec succès.");

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);

        return Result.Success();
    }
}