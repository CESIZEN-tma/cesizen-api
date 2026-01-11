using Bogus;
using api.CZ.Features.Users.Models;
using api.CZ.Features.Sessions.Models;
using api.CZ.Features.EmailConfirmationTokens.Models;
using api.CZ.Features.PasswordResetTokens.Models;

namespace api.Tests.Builders;

/// <summary>
/// Test data builders using Bogus for generating realistic test data
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker Faker = new();

    public static class Users
    {
        public static User Build(Action<User>? configure = null)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = Faker.Internet.Email(),
                PasswordHash = Faker.Random.AlphaNumeric(60),
                FirstName = Faker.Name.FirstName(),
                LastName = Faker.Name.LastName(),
                MemberSince = Faker.Date.Past(2),
                ThumbnailUrl = Faker.Internet.Avatar(),
                LockedUntil = null,
                AccountActivated = true,
                Active = true,
                CreationTime = DateTime.UtcNow,
                UpdateTime = null,
                DeletionTime = null,
                IdUserSavedConfigurations = null
            };

            configure?.Invoke(user);
            return user;
        }

        public static List<User> BuildMany(int count, Action<User>? configure = null)
        {
            return Enumerable.Range(0, count).Select(_ => Build(configure)).ToList();
        }

        public static User BuildInactive()
        {
            return Build(u => u.Active = false);
        }

        public static User BuildUnactivated()
        {
            return Build(u => u.AccountActivated = false);
        }

        public static User BuildLocked(DateTime? until = null)
        {
            return Build(u => u.LockedUntil = until ?? DateTime.UtcNow.AddHours(1));
        }

        public static User BuildDeleted()
        {
            return Build(u => u.DeletionTime = DateTime.UtcNow.AddMinutes(-10));
        }
    }

    public static class Sessions
    {
        public static Session Build(Guid? userId = null, Action<Session>? configure = null)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Token = Faker.Random.AlphaNumeric(64),
                Consumed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreationTime = DateTime.UtcNow,
                UpdateTime = null,
                DeletionTime = null,
                IdUsers = userId ?? Guid.NewGuid()
            };

            configure?.Invoke(session);
            return session;
        }

        public static List<Session> BuildMany(int count, Guid? userId = null, Action<Session>? configure = null)
        {
            return Enumerable.Range(0, count).Select(_ => Build(userId, configure)).ToList();
        }

        public static Session BuildExpired(Guid? userId = null)
        {
            return Build(userId, s => s.ExpiresAt = DateTime.UtcNow.AddDays(-1));
        }

        public static Session BuildConsumed(Guid? userId = null)
        {
            return Build(userId, s => s.Consumed = true);
        }

        public static Session BuildValid(Guid? userId = null)
        {
            return Build(userId, s =>
            {
                s.Consumed = false;
                s.ExpiresAt = DateTime.UtcNow.AddDays(7);
            });
        }
    }

    public static class EmailConfirmationTokens
    {
        public static EmailConfirmationToken Build(Guid? userId = null, Action<EmailConfirmationToken>? configure = null)
        {
            var token = new EmailConfirmationToken
            {
                Id = Guid.NewGuid(),
                Token = Faker.Random.AlphaNumeric(64),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Consumed = false,
                ConsumedAt = null,
                CreationTime = DateTime.UtcNow,
                UpdateTime = null,
                DeletionTime = null,
                IdUsers = userId ?? Guid.NewGuid()
            };

            configure?.Invoke(token);
            return token;
        }

        public static List<EmailConfirmationToken> BuildMany(int count, Guid? userId = null, Action<EmailConfirmationToken>? configure = null)
        {
            return Enumerable.Range(0, count).Select(_ => Build(userId, configure)).ToList();
        }

        public static EmailConfirmationToken BuildExpired(Guid? userId = null)
        {
            return Build(userId, t => t.ExpiresAt = DateTime.UtcNow.AddHours(-1));
        }

        public static EmailConfirmationToken BuildConsumed(Guid? userId = null)
        {
            return Build(userId, t =>
            {
                t.Consumed = true;
                t.ConsumedAt = DateTime.UtcNow.AddHours(-1);
            });
        }

        public static EmailConfirmationToken BuildValid(Guid? userId = null)
        {
            return Build(userId, t =>
            {
                t.Consumed = false;
                t.ExpiresAt = DateTime.UtcNow.AddHours(12);
            });
        }
    }

    public static class PasswordResetTokens
    {
        public static PasswordResetToken Build(Guid? userId = null, Action<PasswordResetToken>? configure = null)
        {
            var token = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = Faker.Random.AlphaNumeric(64),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Consumed = false,
                ConsumedAt = null,
                CreationTime = DateTime.UtcNow,
                UpdateTime = null,
                DeletionTime = null,
                IdUsers = userId ?? Guid.NewGuid()
            };

            configure?.Invoke(token);
            return token;
        }

        public static List<PasswordResetToken> BuildMany(int count, Guid? userId = null, Action<PasswordResetToken>? configure = null)
        {
            return Enumerable.Range(0, count).Select(_ => Build(userId, configure)).ToList();
        }

        public static PasswordResetToken BuildExpired(Guid? userId = null)
        {
            return Build(userId, t => t.ExpiresAt = DateTime.UtcNow.AddMinutes(-10));
        }

        public static PasswordResetToken BuildConsumed(Guid? userId = null)
        {
            return Build(userId, t =>
            {
                t.Consumed = true;
                t.ConsumedAt = DateTime.UtcNow.AddMinutes(-5);
            });
        }

        public static PasswordResetToken BuildValid(Guid? userId = null)
        {
            return Build(userId, t =>
            {
                t.Consumed = false;
                t.ExpiresAt = DateTime.UtcNow.AddMinutes(30);
            });
        }
    }
}
