using System.Security.Claims;
using api.CZ.Core.ResultPattern;
using api.CZ.Core.Services;
using api.CZ.Features.AdminEmailConfirmationTokens.Services;
using api.CZ.Features.AdminPasswordResetTokens.Services;
using api.CZ.Features.AdminSessions.Services;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;
using Simply.Auth.Core.Abstractions;
using Simply.Auth.Core.Enums;

namespace api.CZ.Features.Authentifications.Services;

public class AdminAuthentificationService : IAdminAuthentificationService
{
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutDurationMinutes = 15;

    private readonly IAdministratorRepository _administratorRepository;
    private readonly ISimplyAuthService _simplyAuthService;
    private readonly IAdministratorFactory _administratorFactory;
    private readonly IEmailService _emailService;
    private readonly IAdminEmailConfirmationTokenService _emailConfirmationTokenService;
    private readonly IAdminPasswordResetTokenService _passwordResetTokenService;
    private readonly IAdminSessionService _sessionService;
    private readonly ILogger<AdminAuthentificationService> _logger;

    public AdminAuthentificationService(
        IAdministratorRepository administratorRepository,
        ISimplyAuthService simplyAuthService,
        IAdministratorFactory administratorFactory,
        IEmailService emailService,
        IAdminEmailConfirmationTokenService emailConfirmationTokenService,
        IAdminPasswordResetTokenService passwordResetTokenService,
        IAdminSessionService sessionService,
        ILogger<AdminAuthentificationService> logger)
    {
        _administratorRepository = administratorRepository;
        _simplyAuthService = simplyAuthService;
        _administratorFactory = administratorFactory;
        _emailService = emailService;
        _emailConfirmationTokenService = emailConfirmationTokenService;
        _passwordResetTokenService = passwordResetTokenService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<Result> RegisterAdmin(RegisterDto dto)
    {
        _logger.LogInformation("Admin registration attempt for email {Email}", dto.Email);

        if (dto.Password != dto.ConfirmPassword)
        {
            _logger.LogWarning("Admin registration failed: password mismatch for {Email}", dto.Email);
            return Result.Failure("Password must be identical.");
        }

        if (await _administratorRepository.AnyAsync(a => a.Email == dto.Email))
        {
            _logger.LogWarning("Admin registration failed: email already exists {Email}", dto.Email);
            return Result.Failure("Email already exists");
        }

        var hash = _simplyAuthService.HashPassword(dto.Password);

        Administrator newAdminAccount = _administratorFactory.Create(dto.Email, dto.FirstName, dto.LastName, hash);
        newAdminAccount.MemberSince = DateTime.UtcNow;

        await _administratorRepository.AddAsync(newAdminAccount);

        var confirmationToken = await _emailConfirmationTokenService.NewToken(newAdminAccount.Id);

        await _emailService.SendRegisteringConfirmationEmail(
            confirmationToken.Token,
            newAdminAccount.FirstName,
            newAdminAccount.LastName,
            newAdminAccount.Email,
            "Confirmation de création de compte administrateur",
            "Confirmez votre compte administrateur");

        _logger.LogInformation("Administrator registered successfully: {AdminId}", newAdminAccount.Id);

        return Result.Success();
    }

    public async Task<Result<SimplyAuthResponse>> Login(LoginDto dto)
    {
        _logger.LogInformation("Admin login attempt for email {Email}", dto.Email);

        var admin = await _administratorRepository.FirstOrDefaultAsync(a => a.Email == dto.Email);

        if (admin is null)
        {
            _logger.LogWarning("Admin login failed: administrator not found for {Email}", dto.Email);
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        // Check if account is locked
        if (admin.LockedUntil.HasValue && admin.LockedUntil.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((admin.LockedUntil.Value - DateTime.UtcNow).TotalMinutes);
            _logger.LogWarning("Admin login failed: account locked for {AdminId}, {Minutes} minutes remaining", admin.Id, remainingMinutes);
            return Result.Failure<SimplyAuthResponse>($"Account is locked. Please try again in {remainingMinutes} minute(s).");
        }

        if (!admin.AccountActivated)
        {
            _logger.LogWarning("Admin login failed: account not activated for {AdminId}", admin.Id);
            return Result.Failure<SimplyAuthResponse>("Le compte doit être activé.");
        }

        var result = _simplyAuthService.VerifyPassword(dto.Password, admin.PasswordHash);

        if (result == SimplyVerificationResult.Failed)
        {
            // Increment failed login attempts
            admin.FailedLoginAttempts++;
            admin.UpdateTime = DateTime.UtcNow;

            if (admin.FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                admin.LockedUntil = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                _logger.LogWarning("Admin account locked for {AdminId} after {Attempts} failed attempts", admin.Id, admin.FailedLoginAttempts);
            }

            await _administratorRepository.UpdateAsync(admin);
            _logger.LogWarning("Admin login failed: invalid password for {AdminId}, attempt {Attempt}", admin.Id, admin.FailedLoginAttempts);
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        // Reset failed login attempts on successful login
        if (admin.FailedLoginAttempts > 0 || admin.LockedUntil.HasValue)
        {
            admin.FailedLoginAttempts = 0;
            admin.LockedUntil = null;
            admin.UpdateTime = DateTime.UtcNow;
            await _administratorRepository.UpdateAsync(admin);
        }

        if (result == SimplyVerificationResult.SuccessRehashNeeded)
        {
            var newHash = _simplyAuthService.HashPassword(dto.Password);
            admin.PasswordHash = newHash;
            await _administratorRepository.UpdateAsync(admin);
            _logger.LogInformation("Password rehashed for administrator {AdminId}", admin.Id);
        }

        // Generate tokens with admin role claim
        var tokens = _simplyAuthService.GenerateTokens(admin.Id.ToString(), new[]
        {
            new Claim(ClaimTypes.Role, "Administrator")
        });

        // Create session for refresh token
        await _sessionService.CreateSession(
            admin.Id,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiration);

        _logger.LogInformation("Administrator logged in successfully: {AdminId}", admin.Id);

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
        _logger.LogInformation("Admin account confirmation attempt with token {Token}", token);

        var confirmationToken = await _emailConfirmationTokenService.GetEntityByToken(token);

        if (confirmationToken is null)
        {
            _logger.LogWarning("Admin account confirmation failed: token not found");
            return Result.Failure("Invalid token.");
        }

        if (confirmationToken.Consumed)
        {
            _logger.LogWarning("Admin account confirmation failed: token already consumed for admin {AdminId}", confirmationToken.IdAdministrators);
            return Result.Failure("Token already used.");
        }

        if (confirmationToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Admin account confirmation failed: token expired for admin {AdminId}", confirmationToken.IdAdministrators);
            return Result.Failure("Token expired.");
        }

        var admin = await _administratorRepository.FindAsync(confirmationToken.IdAdministrators);

        if (admin is null)
        {
            _logger.LogError("Admin account confirmation failed: admin {AdminId} not found for valid token", confirmationToken.IdAdministrators);
            return Result.Failure("Administrator not found.");
        }

        if (admin.AccountActivated)
        {
            _logger.LogWarning("Admin account confirmation attempted on already activated account {AdminId}", admin.Id);
            await _emailConfirmationTokenService.Consume(token);
            return Result.Success();
        }

        admin.AccountActivated = true;
        admin.UpdateTime = DateTime.UtcNow;

        await _administratorRepository.UpdateAsync(admin);
        await _emailConfirmationTokenService.Consume(token);

        _logger.LogInformation("Admin account activated successfully for administrator {AdminId}", admin.Id);

        return Result.Success();
    }

    public async Task<Result> ForgotPassword(ForgotPasswordDto dto)
    {
        _logger.LogInformation("Admin password reset request for email {Email}", dto.Email);

        var admin = await _administratorRepository.FirstOrDefaultAsync(a => a.Email == dto.Email);

        if (admin is null)
        {
            _logger.LogWarning("Admin password reset requested for non-existent email {Email}", dto.Email);
            await Task.Delay(Random.Shared.Next(100, 300));
            return Result.Success();
        }

        if (!admin.AccountActivated)
        {
            _logger.LogWarning("Admin password reset requested for unactivated account {AdminId}", admin.Id);
            return Result.Success();
        }

        var resetToken = await _passwordResetTokenService.NewToken(admin.Id);

        var emailResult = await _emailService.SendPasswordResetEmail(
            resetToken.Token,
            admin.FirstName,
            admin.LastName,
            admin.Email,
            "Réinitialisation de votre mot de passe administrateur",
            "Vous avez demandé à réinitialiser votre mot de passe administrateur.",
            TimeSpan.FromMinutes(15));

        if (!emailResult.IsSuccess)
        {
            _logger.LogError("Failed to send admin password reset email to {Email}: {Error}",
                admin.Email, emailResult.Error);
            return Result.Failure("Failed to send reset email. Please try again later.");
        }

        _logger.LogInformation("Admin password reset email sent successfully to {AdminId}", admin.Id);
        return Result.Success();
    }

    public async Task<Result> ResetPassword(ResetPasswordDto dto)
    {
        _logger.LogInformation("Admin password reset attempt with token");

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            _logger.LogWarning("Admin password reset failed: password mismatch");
            return Result.Failure("Passwords must match.");
        }

        var resetToken = await _passwordResetTokenService.GetEntityByToken(dto.Token);

        if (resetToken is null)
        {
            _logger.LogWarning("Admin password reset failed: invalid or expired token");
            return Result.Failure("Invalid or expired reset token.");
        }

        if (resetToken.Consumed)
        {
            _logger.LogWarning("Admin password reset failed: token already used for admin {AdminId}",
                resetToken.IdAdministrators);
            return Result.Failure("This reset link has already been used.");
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Admin password reset failed: token expired for admin {AdminId}",
                resetToken.IdAdministrators);
            return Result.Failure("This reset link has expired. Please request a new one.");
        }

        var admin = await _administratorRepository.FindAsync(resetToken.IdAdministrators);

        if (admin is null)
        {
            _logger.LogError("Admin password reset failed: admin {AdminId} not found for valid token",
                resetToken.IdAdministrators);
            return Result.Failure("Administrator not found.");
        }

        var newHash = _simplyAuthService.HashPassword(dto.NewPassword);

        admin.PasswordHash = newHash;
        admin.UpdateTime = DateTime.UtcNow;

        await _administratorRepository.UpdateAsync(admin);

        await _passwordResetTokenService.Consume(dto.Token);

        await _emailService.SendPasswordResetConfirmationEmail(
            admin.FirstName,
            admin.LastName,
            admin.Email,
            "Votre mot de passe administrateur a été modifié",
            "Votre mot de passe administrateur a été modifié avec succès.");

        _logger.LogInformation("Admin password reset successful for administrator {AdminId}", admin.Id);

        return Result.Success();
    }

    public async Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto)
    {
        _logger.LogInformation("Admin refresh token attempt");

        var session = await _sessionService.GetByRefreshToken(dto.RefreshToken);

        if (session is null)
        {
            _logger.LogWarning("Admin refresh token attempt with invalid or expired token");
            return Result.Failure<SimplyAuthResponse>("Invalid or expired refresh token.");
        }

        var admin = await _administratorRepository.FindAsync(session.IdAdministrators);

        if (admin is null)
        {
            _logger.LogError("Administrator {AdminId} not found for valid session", session.IdAdministrators);
            return Result.Failure<SimplyAuthResponse>("Administrator not found.");
        }

        if (!admin.AccountActivated)
        {
            _logger.LogWarning("Admin refresh token attempt for unactivated account {AdminId}", admin.Id);
            return Result.Failure<SimplyAuthResponse>("Account is not activated.");
        }

        await _sessionService.ConsumeSession(dto.RefreshToken);

        // Generate new tokens with admin role claim
        var tokens = _simplyAuthService.GenerateTokens(admin.Id.ToString(), new[]
        {
            new Claim(ClaimTypes.Role, "Administrator")
        });

        await _sessionService.CreateSession(
            admin.Id,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiration);

        _logger.LogInformation("Admin token refreshed successfully for administrator {AdminId}", admin.Id);

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
        _logger.LogInformation("Admin logout attempt");

        var consumed = await _sessionService.ConsumeSession(refreshToken);

        if (!consumed)
        {
            _logger.LogWarning("Admin logout attempted with invalid or already consumed refresh token");
            return Result.Success();
        }

        _logger.LogInformation("Administrator logged out successfully");

        return Result.Success();
    }

    public async Task<Result<List<SessionDto>>> GetActiveSessions(Guid adminId, string currentRefreshToken)
    {
        _logger.LogInformation("Retrieving active sessions for admin {AdminId}", adminId);

        var sessions = await _sessionService.GetActiveSessionsByAdminId(adminId);
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

    public async Task<Result> RevokeSession(Guid adminId, Guid sessionId)
    {
        _logger.LogInformation("Revoking session {SessionId} for admin {AdminId}", sessionId, adminId);

        var revoked = await _sessionService.RevokeSessionForAdmin(sessionId, adminId);

        if (!revoked)
        {
            _logger.LogWarning("Session {SessionId} not found for admin {AdminId}", sessionId, adminId);
            return Result.Failure("Session not found.");
        }

        return Result.Success();
    }

    public async Task<Result> RevokeAllOtherSessions(Guid adminId, string currentRefreshToken)
    {
        _logger.LogInformation("Revoking all other sessions for admin {AdminId}", adminId);

        var currentSession = await _sessionService.GetByRefreshToken(currentRefreshToken);

        if (currentSession == null)
        {
            _logger.LogWarning("Current session not found for admin {AdminId}", adminId);
            return Result.Failure("Current session not found.");
        }

        await _sessionService.RevokeAllSessionsExceptCurrent(adminId, currentSession.Id);

        return Result.Success();
    }

    public async Task<Result> ChangePassword(Guid adminId, ChangePasswordDto dto, string currentRefreshToken)
    {
        _logger.LogInformation("Password change attempt for admin {AdminId}", adminId);

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            _logger.LogWarning("Password change failed: passwords don't match for admin {AdminId}", adminId);
            return Result.Failure("New passwords must match.");
        }

        var admin = await _administratorRepository.FindAsync(adminId);

        if (admin == null)
        {
            _logger.LogError("Password change failed: admin {AdminId} not found", adminId);
            return Result.Failure("Administrator not found.");
        }

        var verifyResult = _simplyAuthService.VerifyPassword(dto.CurrentPassword, admin.PasswordHash);

        if (verifyResult == Simply.Auth.Core.Enums.SimplyVerificationResult.Failed)
        {
            _logger.LogWarning("Password change failed: current password incorrect for admin {AdminId}", adminId);
            return Result.Failure("Current password is incorrect.");
        }

        // Hash the new password
        var newHash = _simplyAuthService.HashPassword(dto.NewPassword);

        admin.PasswordHash = newHash;
        admin.UpdateTime = DateTime.UtcNow;

        await _administratorRepository.UpdateAsync(admin);

        // Revoke all other sessions for security
        var currentSession = await _sessionService.GetByRefreshToken(currentRefreshToken);
        if (currentSession != null)
        {
            await _sessionService.RevokeAllSessionsExceptCurrent(adminId, currentSession.Id);
        }

        // Send confirmation email
        await _emailService.SendPasswordResetConfirmationEmail(
            admin.FirstName,
            admin.LastName,
            admin.Email,
            "Votre mot de passe administrateur a été modifié",
            "Votre mot de passe administrateur a été modifié avec succès.");

        _logger.LogInformation("Password changed successfully for admin {AdminId}", adminId);

        return Result.Success();
    }
}
