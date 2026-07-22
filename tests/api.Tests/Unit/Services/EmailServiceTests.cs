using FluentAssertions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Moq;
using api.CZ.Core.Services;

namespace api.Tests.Unit.Services;

public class EmailServiceTests
{
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        Environment.SetEnvironmentVariable("URL_FRONT", "https://app.cesizen-test.fr");

        _mockEmailSender = new Mock<IEmailSender>();
        _sut = new EmailService(_mockEmailSender.Object);
    }

    [Fact]
    public void Constructor_UrlFrontNotSet_ThrowsKeyNotFoundException()
    {
        // Arrange
        var previous = Environment.GetEnvironmentVariable("URL_FRONT");
        Environment.SetEnvironmentVariable("URL_FRONT", null);

        try
        {
            // Act
            var act = () => new EmailService(_mockEmailSender.Object);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }
        finally
        {
            Environment.SetEnvironmentVariable("URL_FRONT", previous);
        }
    }

    [Fact]
    public async Task SendRegisteringConfirmationEmail_ValidParameters_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        string? capturedEmail = null, capturedSubject = null, capturedHtml = null;
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((email, subject, html) =>
            {
                capturedEmail = email;
                capturedSubject = subject;
                capturedHtml = html;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SendRegisteringConfirmationEmail(
            confirmationToken: "123456",
            firstName: "Marie",
            lastName: "Dupont",
            receiverEmail: "marie.dupont@example.com",
            subject: "Confirmez votre compte",
            message: "Bienvenue",
            linkExpiration: TimeSpan.FromHours(3));

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedEmail.Should().Be("marie.dupont@example.com");
        capturedSubject.Should().Be("Confirmez votre compte");
        capturedHtml.Should().Contain("123456");
        capturedHtml.Should().Contain("Marie Dupont");
        capturedHtml.Should().Contain("3 heures");
    }

    [Fact]
    public async Task SendRegisteringConfirmationEmail_SenderThrows_ReturnsFailure()
    {
        // Arrange
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP unreachable"));

        // Act
        var result = await _sut.SendRegisteringConfirmationEmail(
            confirmationToken: "654321",
            firstName: "Marie",
            lastName: "Dupont",
            receiverEmail: "marie.dupont@example.com",
            subject: "Confirmez votre compte",
            message: "Bienvenue");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SMTP unreachable");
    }

    [Fact]
    public async Task SendAdministratorCreationEmail_ValidParameters_SendsEmailWithLoginLink()
    {
        // Arrange
        string? capturedHtml = null;
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, html) => capturedHtml = html)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SendAdministratorCreationEmail(
            newAdminFirstName: "Jean",
            newAdminLastName: "Martin",
            receiverEmail: "jean.martin@example.com",
            subject: "Compte administrateur créé",
            message: "Vous êtes désormais administrateur.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedHtml.Should().Contain("https://app.cesizen-test.fr/login");
        capturedHtml.Should().Contain("Jean Martin");
        _mockEmailSender.Verify(
            s => s.SendEmailAsync("jean.martin@example.com", "Compte administrateur créé", It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAdministratorCreationEmail_SenderThrows_ReturnsFailure()
    {
        // Arrange
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("boom"));

        // Act
        var result = await _sut.SendAdministratorCreationEmail(
            "Jean", "Martin", "jean.martin@example.com", "subject", "message");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SendPasswordResetEmail_ValidParameters_IncludesEscapedTokenInResetLink()
    {
        // Arrange
        string? capturedHtml = null;
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, html) => capturedHtml = html)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SendPasswordResetEmail(
            resetToken: "token with space",
            firstName: "Léa",
            lastName: "Bernard",
            email: "lea.bernard@example.com",
            subject: "Réinitialisation",
            message: "Cliquez ci-dessous",
            linkExpiration: TimeSpan.FromMinutes(30));

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedHtml.Should().Contain("https://app.cesizen-test.fr/reset-password?token=token%20with%20space");
        capturedHtml.Should().Contain("30 minutes");
    }

    [Fact]
    public async Task SendPasswordResetEmail_SenderThrows_ReturnsFailure()
    {
        // Arrange
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("network error"));

        // Act
        var result = await _sut.SendPasswordResetEmail(
            "token", "Léa", "Bernard", "lea.bernard@example.com", "subject", "message");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("network error");
    }

    [Fact]
    public async Task SendPasswordResetConfirmationEmail_ValidParameters_SendsSecurityAlert()
    {
        // Arrange
        string? capturedHtml = null;
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, html) => capturedHtml = html)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SendPasswordResetConfirmationEmail(
            firstName: "Thomas",
            lastName: "Martin",
            email: "thomas.martin@example.com",
            subject: "Mot de passe modifié",
            message: "Votre mot de passe a changé.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedHtml.Should().Contain("Alerte de sécurité");
        capturedHtml.Should().Contain("https://app.cesizen-test.fr/login");
    }

    [Fact]
    public async Task SendPasswordResetConfirmationEmail_SenderThrows_ReturnsFailure()
    {
        // Arrange
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("timeout"));

        // Act
        var result = await _sut.SendPasswordResetConfirmationEmail(
            "Thomas", "Martin", "thomas.martin@example.com", "subject", "message");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, "1 jour")]
    [InlineData(3, "3 jours")]
    public async Task SendPasswordResetEmail_MultiDayExpiration_FormatsPluralCorrectly(int days, string expected)
    {
        // Arrange
        string? capturedHtml = null;
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, html) => capturedHtml = html)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.SendPasswordResetEmail(
            "token", "Léa", "Bernard", "lea.bernard@example.com", "subject", "message",
            linkExpiration: TimeSpan.FromDays(days));

        // Assert
        capturedHtml.Should().Contain(expected);
    }

    [Fact]
    public async Task SendRegisteringConfirmationEmail_NoExpirationProvided_OmitsExpirationSection()
    {
        // Arrange
        string? capturedHtml = null;
        _mockEmailSender
            .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, html) => capturedHtml = html)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.SendRegisteringConfirmationEmail(
            "111111", "Marie", "Dupont", "marie.dupont@example.com", "subject", "message");

        // Assert
        capturedHtml.Should().NotContain("Ce code expire dans");
    }
}
