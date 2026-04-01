using System;
using System.Collections.Generic;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.AdminEmailConfirmationTokens.Models;
using api.CZ.Features.AdminLogs.Models;
using api.CZ.Features.AdminPasswordResetTokens.Models;
using api.CZ.Features.AdminSessions.Models;
using api.CZ.Features.Bookmarks.Models;
using api.CZ.Features.Configurations.Models;
using api.CZ.Features.EmailConfirmationTokens.Models;
using api.CZ.Features.InformationPages.Models;
using api.CZ.Features.InformationTags.Models;
using api.CZ.Features.NavigationMenus.Models;
using api.CZ.Features.PasswordResetTokens.Models;
using api.CZ.Features.PasswordHistories.Models;
using api.CZ.Features.PasswordsInfos.Models;
using api.CZ.Features.Quizzes.Models;
using api.CZ.Features.Users.Models;
using api.CZ.Features.UserSavedConfigurations.Models;
using Microsoft.EntityFrameworkCore;
using Session = api.CZ.Features.Sessions.Models.Session;

namespace api.CZ.Data.EFCore;

public partial class CesiZenDbContext : DbContext
{
    public CesiZenDbContext(DbContextOptions<CesiZenDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminLog> AdminLogs { get; set; }

    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<AdminEmailConfirmationToken> AdminEmailConfirmationTokens { get; set; }

    public virtual DbSet<AdminPasswordResetToken> AdminPasswordResetTokens { get; set; }

    public virtual DbSet<AdminSession> AdminSessions { get; set; }

    public virtual DbSet<api.CZ.Features.Bookmarks.Models.Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Configuration> Configurations { get; set; }

    public virtual DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }

    public virtual DbSet<InformationPage> InformationPages { get; set; }

    public virtual DbSet<InformationTag> InformationTags { get; set; }

    public virtual DbSet<NavigationMenu> NavigationMenus { get; set; }

    public virtual DbSet<PasswordHistory> PasswordHistories { get; set; }

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<PasswordsInfo> PasswordsInfos { get; set; }

    public virtual DbSet<api.CZ.Features.Quizzes.Models.Question> Questions { get; set; }

    public virtual DbSet<api.CZ.Features.Quizzes.Models.Quizz> Quizzs { get; set; }

    public virtual DbSet<api.CZ.Features.Quizzes.Models.ResponsesOption> ResponsesOptions { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSavedConfiguration> UserSavedConfigurations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_logs_pk");

            entity.ToTable("admin_logs");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ActionCode).HasColumnName("action_code");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EntityType)
                .HasMaxLength(255)
                .HasColumnName("entity_type");
            entity.Property(e => e.IdAdministrator).HasColumnName("id_administrator");
            entity.Property(e => e.TargetedEntityId).HasColumnName("targeted_entity_id");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdAdministratorNavigation).WithMany(p => p.AdminLogs)
                .HasForeignKey(d => d.IdAdministrator)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admin_logs_id_administrator_fk");
        });

        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("administrators_pk");

            entity.ToTable("administrators");

            entity.HasIndex(e => e.Email, "email_admin_idx");

            entity.HasIndex(e => e.Email, "email_admin_unq").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountActivated).HasColumnName("account_activated");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.IdNavigationMenu).HasColumnName("id_navigation_menu");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.LockedUntil).HasColumnName("locked_until");
            entity.Property(e => e.MemberSince).HasColumnName("member_since");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdNavigationMenuNavigation).WithMany(p => p.Administrators)
                .HasForeignKey(d => d.IdNavigationMenu)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("administrators_id_navigation_menu_fk");

            entity.HasMany(d => d.IdSessions).WithMany(p => p.Ids)
                .UsingEntity<Dictionary<string, object>>(
                    "Auth",
                    r => r.HasOne<Session>().WithMany()
                        .HasForeignKey("IdSession")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("auth_id_session_fk"),
                    l => l.HasOne<Administrator>().WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("auth_id_fk"),
                    j =>
                    {
                        j.HasKey("Id", "IdSession").HasName("auth_pk");
                        j.ToTable("auth");
                        j.IndexerProperty<Guid>("Id").HasColumnName("id");
                        j.IndexerProperty<Guid>("IdSession").HasColumnName("id_session");
                    });
        });

        modelBuilder.Entity<AdminEmailConfirmationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_email_confirmation_tokens_pk");

            entity.ToTable("admin_email_confirmation_tokens");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.ConsumedAt).HasColumnName("consumed_at");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdAdministrators).HasColumnName("id_administrators");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdAdministratorsNavigation).WithMany(p => p.AdminEmailConfirmationTokens)
                .HasForeignKey(d => d.IdAdministrators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admin_email_confirmation_tokens_id_administrators_fk");
        });

        modelBuilder.Entity<AdminPasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_password_reset_tokens_pk");

            entity.ToTable("admin_password_reset_tokens");

            entity.HasIndex(e => e.Token, "admin_token_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.ConsumedAt).HasColumnName("consumed_at");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdAdministrators).HasColumnName("id_administrators");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdAdministratorsNavigation).WithMany(p => p.AdminPasswordResetTokens)
                .HasForeignKey(d => d.IdAdministrators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admin_password_reset_tokens_id_administrators_fk");
        });

        modelBuilder.Entity<AdminSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("admin_session_pk");

            entity.ToTable("admin_session");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdAdministrators).HasColumnName("id_administrators");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdAdministratorsNavigation).WithMany(p => p.AdminSessions)
                .HasForeignKey(d => d.IdAdministrators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admin_session_id_administrators_fk");
        });

        modelBuilder.Entity<api.CZ.Features.Bookmarks.Models.Bookmark>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.IdConfigurations }).HasName("bookmark_pk");

            entity.ToTable("bookmark");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IdConfigurations).HasColumnName("id_configurations");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdNavigation).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookmark_id_fk");

            entity.HasOne(d => d.IdConfigurationsNavigation).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.IdConfigurations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookmark_id_configurations_fk");
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("configurations_pk");

            entity.ToTable("configurations");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Difficulty).HasColumnName("difficulty");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Exhalation).HasColumnName("exhalation");
            entity.Property(e => e.GuidanceType)
                .HasMaxLength(50)
                .HasColumnName("guidance_type");
            entity.Property(e => e.IdAdministrators).HasColumnName("id_administrators");
            entity.Property(e => e.Inhalation).HasColumnName("inhalation");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Objective)
                .HasMaxLength(50)
                .HasColumnName("objective");
            entity.Property(e => e.Retention1).HasColumnName("retention1");
            entity.Property(e => e.Retention2).HasColumnName("retention2");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdAdministratorsNavigation).WithMany(p => p.Configurations)
                .HasForeignKey(d => d.IdAdministrators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("configurations_id_administrators_fk");
        });

        modelBuilder.Entity<EmailConfirmationToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("email_confirmation_tokens_pk");

            entity.ToTable("email_confirmation_tokens");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.ConsumedAt).HasColumnName("consumed_at");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdUsers).HasColumnName("id_users");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdUsersNavigation).WithMany(p => p.EmailConfirmationTokens)
                .HasForeignKey(d => d.IdUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("email_confirmation_tokens_id_users_fk");
        });

        modelBuilder.Entity<InformationPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("information_pages_pk");

            entity.ToTable("information_pages");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ContentType)
                .HasMaxLength(255)
                .HasColumnName("content_type");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.CurrentlyEditing).HasColumnName("currently_editing");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IdAdministrators).HasColumnName("id_administrators");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdAdministratorsNavigation).WithMany(p => p.InformationPages)
                .HasForeignKey(d => d.IdAdministrators)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("information_pages_id_administrators_fk");

            entity.HasMany(d => d.IdInformationTags).WithMany(p => p.Ids)
                .UsingEntity<Dictionary<string, object>>(
                    "Tagged",
                    r => r.HasOne<InformationTag>().WithMany()
                        .HasForeignKey("IdInformationTags")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("tagged_id_information_tags_fk"),
                    l => l.HasOne<InformationPage>().WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("tagged_id_fk"),
                    j =>
                    {
                        j.HasKey("Id", "IdInformationTags").HasName("tagged_pk");
                        j.ToTable("tagged");
                        j.IndexerProperty<Guid>("Id").HasColumnName("id");
                        j.IndexerProperty<Guid>("IdInformationTags").HasColumnName("id_information_tags");
                    });
        });

        modelBuilder.Entity<InformationTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("information_tags_pk");

            entity.ToTable("information_tags");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Label)
                .HasMaxLength(255)
                .HasColumnName("label");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");
        });

        modelBuilder.Entity<NavigationMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("navigation_menu_pk");

            entity.ToTable("navigation_menu");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.CurrentlyEditing).HasColumnName("currently_editing");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Label)
                .HasMaxLength(100)
                .HasColumnName("label");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");
            entity.Property(e => e.Url).HasColumnName("url");

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("password_history_pk");

            entity.ToTable("password_history");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.IdPasswordsInfos).HasColumnName("id_passwords_infos");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdPasswordsInfosNavigation).WithMany(p => p.PasswordHistories)
                .HasForeignKey(d => d.IdPasswordsInfos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("password_history_id_passwords_infos_fk");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("password_reset_tokens_pk");

            entity.ToTable("password_reset_tokens");

            entity.HasIndex(e => e.Token, "token_idx");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.ConsumedAt).HasColumnName("consumed_at");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdUsers).HasColumnName("id_users");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdUsersNavigation).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.IdUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("password_reset_tokens_id_users_fk");
        });

        modelBuilder.Entity<PasswordsInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("passwords_infos_pk");

            entity.ToTable("passwords_infos");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AttemptCount).HasColumnName("attempt_count");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
            entity.Property(e => e.LastReset).HasColumnName("last_reset");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");
        });

        modelBuilder.Entity<api.CZ.Features.Quizzes.Models.Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("questions_pk");

            entity.ToTable("questions");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.IdQuizz).HasColumnName("id_quizz");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdQuizzNavigation).WithMany(p => p.Questions)
                .HasForeignKey(d => d.IdQuizz)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("questions_id_quizz_fk");
        });

        modelBuilder.Entity<api.CZ.Features.Quizzes.Models.Quizz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("quizz_pk");

            entity.ToTable("quizz");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Nom)
                .HasMaxLength(255)
                .HasColumnName("nom");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");
        });

        modelBuilder.Entity<api.CZ.Features.Quizzes.Models.ResponsesOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("responses_options_pk");

            entity.ToTable("responses_options");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.IdQuestions).HasColumnName("id_questions");
            entity.Property(e => e.Label)
                .HasMaxLength(255)
                .HasColumnName("label");
            entity.Property(e => e.Operation)
                .HasMaxLength(255)
                .HasColumnName("operation");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.TargetedField)
                .HasMaxLength(255)
                .HasColumnName("targeted_field");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.IdQuestionsNavigation).WithMany(p => p.ResponsesOptions)
                .HasForeignKey(d => d.IdQuestions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("responses_options_id_questions_fk");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("session_pk");

            entity.ToTable("session");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Consumed).HasColumnName("consumed");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.IdUsers).HasColumnName("id_users");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdUsersNavigation).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.IdUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("session_id_users_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pk");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email_idx");

            entity.HasIndex(e => e.Email, "email_unq").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountActivated).HasColumnName("account_activated");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.IdUserSavedConfigurations).HasColumnName("id_user_saved_configurations");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.LockedUntil).HasColumnName("locked_until");
            entity.Property(e => e.MemberSince).HasColumnName("member_since");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");

            entity.HasOne(d => d.IdUserSavedConfigurationsNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdUserSavedConfigurations)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_id_user_saved_configurations_fk");
        });

        modelBuilder.Entity<UserSavedConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_saved_configurations_pk");

            entity.ToTable("user_saved_configurations");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreationTime).HasColumnName("creation_time");
            entity.Property(e => e.DeletionTime).HasColumnName("deletion_time");
            entity.Property(e => e.Difficulty).HasColumnName("difficulty");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Exhalation).HasColumnName("exhalation");
            entity.Property(e => e.GuidanceType)
                .HasMaxLength(50)
                .HasColumnName("guidance_type");
            entity.Property(e => e.Inhalation).HasColumnName("inhalation");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Objective)
                .HasMaxLength(50)
                .HasColumnName("objective");
            entity.Property(e => e.Retention1).HasColumnName("retention1");
            entity.Property(e => e.Retention2).HasColumnName("retention2");
            entity.Property(e => e.UpdateTime).HasColumnName("update_time");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
