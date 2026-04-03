using System.Security.Claims;
using api.CZ.Core.ResultPattern;
using api.CZ.Data.Repositories;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;
using Simply.Auth.Core.Abstractions;
using Simply.Auth.Core.Enums;

namespace api.CZ.Core.Services;

/// <summary>
/// Abstract base class providing common authentication functionality for User and Admin authentication.
/// Eliminates code duplication between AuthentificationService and AdminAuthentificationService.
/// Implements common operations: registration, login, account confirmation, password reset, token refresh, and logout.
/// </summary>
public abstract class BaseAuthenticationService<TEntity, TSession, TEntityRepository, TEntityFactory,
    TTokenService, TResetTokenService, TSessionService> : IBaseAuthenticationService
    where TEntity : class
    where TSession : class
    where TEntityRepository : IBaseRepository<TEntity>
    where TEntityFactory : class
    where TTokenService : class
    where TResetTokenService : class
    where TSessionService : class
{
    private const int MaxFailedLoginAttempts = 5;
    private const int LockoutDurationMinutes = 15;

    protected readonly TEntityRepository EntityRepository;
    protected readonly ISimplyAuthService SimplyAuthService;
    protected readonly TEntityFactory EntityFactory;
    protected readonly IEmailService EmailService;
    protected readonly TTokenService EmailConfirmationTokenService;
    protected readonly TResetTokenService PasswordResetTokenService;
    protected readonly TSessionService SessionService;
    protected readonly ILogger Logger;

    // Delegates for entity-specific operations
    protected readonly Func<TEntity, Guid> GetEntityId;
    protected readonly Func<TEntity, string> GetEmail;
    protected readonly Func<TEntity, string> GetPasswordHash;
    protected readonly Func<TEntity, string> GetFirstName;
    protected readonly Func<TEntity, string> GetLastName;
    protected readonly Func<TEntity, DateTime?> GetLockedUntil;
    protected readonly Func<TEntity, int> GetFailedLoginAttempts;
    protected readonly Func<TEntity, bool> GetAccountActivated;
    protected readonly Func<TEntity, bool?> GetActive; // Nullable because admins don't have this
    protected readonly Action<TEntity, string> SetPasswordHash;
    protected readonly Action<TEntity, DateTime> SetUpdateTime;
    protected readonly Action<TEntity, DateTime?> SetLockedUntil;
    protected readonly Action<TEntity, int> SetFailedLoginAttempts;
    protected readonly Action<TEntity, bool> SetAccountActivated;
    protected readonly Func<string, string, string, string, TEntity> CreateEntityFunc;
    protected readonly Func<Guid, Task<object>> NewEmailConfirmationToken;
    protected readonly Func<string, Task<object?>> GetEntityByEmailConfirmationToken;
    protected readonly Func<string, Task> ConsumeEmailConfirmationToken;
    protected readonly Func<Guid, Task<object>> NewPasswordResetToken;
    protected readonly Func<string, Task<object?>> GetEntityByPasswordResetToken;
    protected readonly Func<string, Task> ConsumePasswordResetToken;
    protected readonly Func<Guid, string, DateTime, Task<TSession>> CreateSession;
    protected readonly Func<string, Task<TSession?>> GetByRefreshToken;
    protected readonly Func<string, Task<bool>> ConsumeSession;
    protected readonly Func<object, Guid> GetTokenEntityId;
    protected readonly Func<object, bool> GetTokenConsumed;
    protected readonly Func<object, DateTime> GetTokenExpiresAt;
    protected readonly Func<object, string> GetTokenValue;
    protected readonly Claim[]? AdditionalClaims;
    protected readonly string EntityTypeName;
    protected readonly string EntityTypeDisplayName;

    protected BaseAuthenticationService(
        TEntityRepository entityRepository,
        ISimplyAuthService simplyAuthService,
        TEntityFactory entityFactory,
        IEmailService emailService,
        TTokenService emailConfirmationTokenService,
        TResetTokenService passwordResetTokenService,
        TSessionService sessionService,
        ILogger logger,
        Func<TEntity, Guid> getEntityId,
        Func<TEntity, string> getEmail,
        Func<TEntity, string> getPasswordHash,
        Func<TEntity, string> getFirstName,
        Func<TEntity, string> getLastName,
        Func<TEntity, DateTime?> getLockedUntil,
        Func<TEntity, int> getFailedLoginAttempts,
        Func<TEntity, bool> getAccountActivated,
        Func<TEntity, bool?> getActive,
        Action<TEntity, string> setPasswordHash,
        Action<TEntity, DateTime> setUpdateTime,
        Action<TEntity, DateTime?> setLockedUntil,
        Action<TEntity, int> setFailedLoginAttempts,
        Action<TEntity, bool> setAccountActivated,
        Func<string, string, string, string, TEntity> createEntityFunc,
        Func<Guid, Task<object>> newEmailConfirmationToken,
        Func<string, Task<object?>> getEntityByEmailConfirmationToken,
        Func<string, Task> consumeEmailConfirmationToken,
        Func<Guid, Task<object>> newPasswordResetToken,
        Func<string, Task<object?>> getEntityByPasswordResetToken,
        Func<string, Task> consumePasswordResetToken,
        Func<Guid, string, DateTime, Task<TSession>> createSession,
        Func<string, Task<TSession?>> getByRefreshToken,
        Func<string, Task<bool>> consumeSession,
        Func<object, Guid> getTokenEntityId,
        Func<object, bool> getTokenConsumed,
        Func<object, DateTime> getTokenExpiresAt,
        Func<object, string> getTokenValue,
        string entityTypeName,
        string entityTypeDisplayName,
        Claim[]? additionalClaims = null)
    {
        EntityRepository = entityRepository;
        SimplyAuthService = simplyAuthService;
        EntityFactory = entityFactory;
        EmailService = emailService;
        EmailConfirmationTokenService = emailConfirmationTokenService;
        PasswordResetTokenService = passwordResetTokenService;
        SessionService = sessionService;
        Logger = logger;
        GetEntityId = getEntityId;
        GetEmail = getEmail;
        GetPasswordHash = getPasswordHash;
        GetFirstName = getFirstName;
        GetLastName = getLastName;
        GetLockedUntil = getLockedUntil;
        GetFailedLoginAttempts = getFailedLoginAttempts;
        GetAccountActivated = getAccountActivated;
        GetActive = getActive;
        SetPasswordHash = setPasswordHash;
        SetUpdateTime = setUpdateTime;
        SetLockedUntil = setLockedUntil;
        SetFailedLoginAttempts = setFailedLoginAttempts;
        SetAccountActivated = setAccountActivated;
        CreateEntityFunc = createEntityFunc;
        NewEmailConfirmationToken = newEmailConfirmationToken;
        GetEntityByEmailConfirmationToken = getEntityByEmailConfirmationToken;
        ConsumeEmailConfirmationToken = consumeEmailConfirmationToken;
        NewPasswordResetToken = newPasswordResetToken;
        GetEntityByPasswordResetToken = getEntityByPasswordResetToken;
        ConsumePasswordResetToken = consumePasswordResetToken;
        CreateSession = createSession;
        GetByRefreshToken = getByRefreshToken;
        ConsumeSession = consumeSession;
        GetTokenEntityId = getTokenEntityId;
        GetTokenConsumed = getTokenConsumed;
        GetTokenExpiresAt = getTokenExpiresAt;
        GetTokenValue = getTokenValue;
        EntityTypeName = entityTypeName;
        EntityTypeDisplayName = entityTypeDisplayName;
        AdditionalClaims = additionalClaims;
    }

    protected async Task<Result> RegisterEntity(RegisterDto dto, Action<TEntity> configureEntity)
    {
        Logger.LogInformation("{EntityType} registration attempt for email {Email}", EntityTypeName, dto.Email);

        if (dto.Password != dto.ConfirmPassword)
        {
            Logger.LogWarning("{EntityType} registration failed: password mismatch for {Email}", EntityTypeName, dto.Email);
            return Result.Failure("Password must be identical.");
        }

        if (await EntityRepository.AnyAsync(e => GetEmail(e) == dto.Email))
        {
            Logger.LogWarning("{EntityType} registration failed: email already exists {Email}", EntityTypeName, dto.Email);
            return Result.Failure("Email already exists");
        }

        var hash = SimplyAuthService.HashPassword(dto.Password);
        TEntity newEntity = CreateEntityFunc(dto.Email, dto.FirstName, dto.LastName, hash);
        configureEntity(newEntity);

        await EntityRepository.AddAsync(newEntity);

        var confirmationToken = await NewEmailConfirmationToken(GetEntityId(newEntity));

        await EmailService.SendRegisteringConfirmationEmail(
            GetTokenValue(confirmationToken),
            GetFirstName(newEntity),
            GetLastName(newEntity),
            GetEmail(newEntity),
            $"Confirmation de création de compte {EntityTypeDisplayName}",
            $"Confirmez votre compte {EntityTypeDisplayName}");

        Logger.LogInformation("{EntityType} registered successfully: {EntityId}", EntityTypeName, GetEntityId(newEntity));

        return Result.Success();
    }

    public async Task<Result<SimplyAuthResponse>> Login(LoginDto dto) => await LoginEntity(dto);

    protected async Task<Result<SimplyAuthResponse>> LoginEntity(LoginDto dto)
    {
        Logger.LogInformation("{EntityType} login attempt for email {Email}", EntityTypeName, dto.Email);

        var entity = await EntityRepository.FirstOrDefaultAsync(e => GetEmail(e) == dto.Email);

        if (entity is null)
        {
            Logger.LogWarning("{EntityType} login failed: entity not found for {Email}", EntityTypeName, dto.Email);
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        // Check if account is locked
        if (GetLockedUntil(entity).HasValue && GetLockedUntil(entity)!.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((GetLockedUntil(entity)!.Value - DateTime.UtcNow).TotalMinutes);
            Logger.LogWarning("{EntityType} login failed: account locked for {EntityId}, {Minutes} minutes remaining",
                EntityTypeName, GetEntityId(entity), remainingMinutes);
            return Result.Failure<SimplyAuthResponse>($"Account is locked. Please try again in {remainingMinutes} minute(s).");
        }

        if (!GetAccountActivated(entity))
        {
            Logger.LogWarning("{EntityType} login failed: account not activated for {EntityId}", EntityTypeName, GetEntityId(entity));
            return Result.Failure<SimplyAuthResponse>("Le compte doit être activé.");
        }

        // Check Active property only if it exists (users have it, admins don't)
        var activeValue = GetActive(entity);
        if (activeValue.HasValue && !activeValue.Value)
        {
            Logger.LogWarning("{EntityType} login failed: account disabled for {EntityId}", EntityTypeName, GetEntityId(entity));
            return Result.Failure<SimplyAuthResponse>("Account has been disabled. Please contact support.");
        }

        var result = SimplyAuthService.VerifyPassword(dto.Password, GetPasswordHash(entity));

        if (result == SimplyVerificationResult.Failed)
        {
            // Increment failed login attempts
            SetFailedLoginAttempts(entity, GetFailedLoginAttempts(entity) + 1);
            SetUpdateTime(entity, DateTime.UtcNow);

            if (GetFailedLoginAttempts(entity) >= MaxFailedLoginAttempts)
            {
                SetLockedUntil(entity, DateTime.UtcNow.AddMinutes(LockoutDurationMinutes));
                Logger.LogWarning("{EntityType} account locked for {EntityId} after {Attempts} failed attempts",
                    EntityTypeName, GetEntityId(entity), GetFailedLoginAttempts(entity));
            }

            await EntityRepository.UpdateAsync(entity);
            Logger.LogWarning("{EntityType} login failed: invalid password for {EntityId}, attempt {Attempt}",
                EntityTypeName, GetEntityId(entity), GetFailedLoginAttempts(entity));
            return Result.Failure<SimplyAuthResponse>("Invalid credentials");
        }

        // Reset failed login attempts on successful login
        if (GetFailedLoginAttempts(entity) > 0 || GetLockedUntil(entity).HasValue)
        {
            SetFailedLoginAttempts(entity, 0);
            SetLockedUntil(entity, null);
            SetUpdateTime(entity, DateTime.UtcNow);
            await EntityRepository.UpdateAsync(entity);
        }

        if (result == SimplyVerificationResult.SuccessRehashNeeded)
        {
            var newHash = SimplyAuthService.HashPassword(dto.Password);
            SetPasswordHash(entity, newHash);
            await EntityRepository.UpdateAsync(entity);
            Logger.LogInformation("Password rehashed for {EntityType} {EntityId}", EntityTypeName, GetEntityId(entity));
        }

        var tokens = AdditionalClaims != null
            ? SimplyAuthService.GenerateTokens(GetEntityId(entity).ToString(), AdditionalClaims)
            : SimplyAuthService.GenerateTokens(GetEntityId(entity).ToString());

        await CreateSession(GetEntityId(entity), tokens.RefreshToken, tokens.RefreshTokenExpiration);

        Logger.LogInformation("{EntityType} logged in successfully: {EntityId}", EntityTypeName, GetEntityId(entity));

        return Result.Success(new SimplyAuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration
        });
    }

    public async Task<Result> ConfirmAccount(string token) => await ConfirmEntityAccount(token);

    protected async Task<Result> ConfirmEntityAccount(string token)
    {
        Logger.LogInformation("{EntityType} account confirmation attempt with token", EntityTypeName);

        var confirmationToken = await GetEntityByEmailConfirmationToken(token);

        if (confirmationToken is null)
        {
            Logger.LogWarning("{EntityType} account confirmation failed: token not found", EntityTypeName);
            return Result.Failure("Invalid token.");
        }

        if (GetTokenConsumed(confirmationToken))
        {
            Logger.LogWarning("{EntityType} account confirmation failed: token already consumed for entity {EntityId}",
                EntityTypeName, GetTokenEntityId(confirmationToken));
            return Result.Failure("Token already used.");
        }

        if (GetTokenExpiresAt(confirmationToken) < DateTime.UtcNow)
        {
            Logger.LogWarning("{EntityType} account confirmation failed: token expired for entity {EntityId}",
                EntityTypeName, GetTokenEntityId(confirmationToken));
            return Result.Failure("Token expired.");
        }

        var entity = await EntityRepository.FindAsync(GetTokenEntityId(confirmationToken));

        if (entity is null)
        {
            Logger.LogError("{EntityType} account confirmation failed: entity {EntityId} not found for valid token",
                EntityTypeName, GetTokenEntityId(confirmationToken));
            return Result.Failure($"{EntityTypeDisplayName} not found.");
        }

        if (GetAccountActivated(entity))
        {
            Logger.LogWarning("{EntityType} account confirmation attempted on already activated account {EntityId}",
                EntityTypeName, GetEntityId(entity));
            await ConsumeEmailConfirmationToken(token);
            return Result.Success();
        }

        SetAccountActivated(entity, true);
        SetUpdateTime(entity, DateTime.UtcNow);

        await EntityRepository.UpdateAsync(entity);
        await ConsumeEmailConfirmationToken(token);

        Logger.LogInformation("{EntityType} account activated successfully for entity {EntityId}", EntityTypeName, GetEntityId(entity));

        return Result.Success();
    }

    public async Task<Result> ForgotPassword(ForgotPasswordDto dto) => await ForgotEntityPassword(dto);

    protected async Task<Result> ForgotEntityPassword(ForgotPasswordDto dto)
    {
        Logger.LogInformation("{EntityType} password reset request for email {Email}", EntityTypeName, dto.Email);

        var entity = await EntityRepository.FirstOrDefaultAsync(e => GetEmail(e) == dto.Email);

        if (entity is null)
        {
            Logger.LogWarning("{EntityType} password reset requested for non-existent email {Email}", EntityTypeName, dto.Email);
            await Task.Delay(Random.Shared.Next(100, 300));
            return Result.Success();
        }

        if (!GetAccountActivated(entity))
        {
            Logger.LogWarning("{EntityType} password reset requested for unactivated account {EntityId}", EntityTypeName, GetEntityId(entity));
            return Result.Success();
        }

        var resetToken = await NewPasswordResetToken(GetEntityId(entity));

        var emailResult = await EmailService.SendPasswordResetEmail(
            GetTokenValue(resetToken),
            GetFirstName(entity),
            GetLastName(entity),
            GetEmail(entity),
            $"Réinitialisation de votre mot de passe {EntityTypeDisplayName}",
            $"Vous avez demandé à réinitialiser votre mot de passe {EntityTypeDisplayName}.",
            TimeSpan.FromMinutes(15));

        if (!emailResult.IsSuccess)
        {
            Logger.LogError("Failed to send {EntityType} password reset email to {Email}: {Error}",
                EntityTypeName, GetEmail(entity), emailResult.Error);
            return Result.Failure("Failed to send reset email. Please try again later.");
        }

        Logger.LogInformation("{EntityType} password reset email sent successfully to {EntityId}", EntityTypeName, GetEntityId(entity));
        return Result.Success();
    }

    public async Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto) => await RefreshEntityToken(dto);

    protected async Task<Result<SimplyAuthResponse>> RefreshEntityToken(RefreshTokenDto dto)
    {
        Logger.LogInformation("{EntityType} refresh token attempt", EntityTypeName);

        var session = await GetByRefreshToken(dto.RefreshToken);

        if (session is null)
        {
            Logger.LogWarning("{EntityType} refresh token attempt with invalid or expired token", EntityTypeName);
            return Result.Failure<SimplyAuthResponse>("Invalid or expired refresh token.");
        }

        var sessionIdProp = session.GetType().GetProperty("IdUsers") ?? session.GetType().GetProperty("IdAdministrators");
        if (sessionIdProp == null)
        {
            Logger.LogError("Unable to determine entity ID from session");
            return Result.Failure<SimplyAuthResponse>("Session error.");
        }

        var entityId = (Guid)sessionIdProp.GetValue(session)!;
        var entity = await EntityRepository.FindAsync(entityId);

        if (entity is null)
        {
            Logger.LogError("{EntityType} {EntityId} not found for valid session", EntityTypeName, entityId);
            return Result.Failure<SimplyAuthResponse>($"{EntityTypeDisplayName} not found.");
        }

        if (!GetAccountActivated(entity))
        {
            Logger.LogWarning("{EntityType} refresh token attempt for unactivated account {EntityId}", EntityTypeName, GetEntityId(entity));
            return Result.Failure<SimplyAuthResponse>("Account is not activated.");
        }

        await ConsumeSession(dto.RefreshToken);

        var tokens = AdditionalClaims != null
            ? SimplyAuthService.GenerateTokens(GetEntityId(entity).ToString(), AdditionalClaims)
            : SimplyAuthService.GenerateTokens(GetEntityId(entity).ToString());

        await CreateSession(GetEntityId(entity), tokens.RefreshToken, tokens.RefreshTokenExpiration);

        Logger.LogInformation("{EntityType} token refreshed successfully for entity {EntityId}", EntityTypeName, GetEntityId(entity));

        return Result.Success(new SimplyAuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiration = tokens.AccessTokenExpiration,
            RefreshTokenExpiration = tokens.RefreshTokenExpiration
        });
    }

    public async Task<Result> Logout(string refreshToken) => await LogoutEntity(refreshToken);

    protected async Task<Result> LogoutEntity(string refreshToken)
    {
        Logger.LogInformation("{EntityType} logout attempt", EntityTypeName);

        var consumed = await ConsumeSession(refreshToken);

        if (!consumed)
        {
            Logger.LogWarning("{EntityType} logout attempted with invalid or already consumed refresh token", EntityTypeName);
            return Result.Success();
        }

        Logger.LogInformation("{EntityType} logged out successfully", EntityTypeName);

        return Result.Success();
    }
}
