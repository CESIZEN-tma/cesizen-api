using api.CZ.Core.ResultPattern;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace api.CZ.Core.Services;

public class EmailService : IEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly string _baseUrl;

    public EmailService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
        _baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") 
            ?? throw new KeyNotFoundException("APP_BASE_URL environment variable is not set");
    }

    public async Task<Result> SendRegisteringConfirmationEmail(
        string confirmationToken,
        string firstName,
        string lastName,
        string receiverEmail,
        string subject,
        string message,
        TimeSpan? linkExpiration = null)
    {
        var confirmationLink = $"{_baseUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";
        
        var htmlContent = BuildEmailTemplate(
            title: "Confirmez votre adresse email",
            greeting: $"Bonjour {firstName} {lastName},",
            mainContent: $"""
                <p>{message}</p>
                <p>Pour activer votre compte, veuillez confirmer votre adresse email en cliquant sur le bouton ci-dessous :</p>
            """,
            buttonText: "Confirmer mon email",
            buttonLink: confirmationLink,
            footerNote: "Si vous n'avez pas créé de compte, vous pouvez ignorer cet email.",
            linkExpiration: linkExpiration
        );

        return await SendEmailSafelyAsync(receiverEmail, subject, htmlContent);
    }

    public async Task<Result> SendAdministratorCreationEmail(
        string newAdminFirstName,
        string newAdminLastName,
        string receiverEmail,
        string subject,
        string message)
    {
        var loginLink = $"{_baseUrl}/login";
        
        var htmlContent = BuildEmailTemplate(
            title: "Compte administrateur créé",
            greeting: $"Bonjour {newAdminFirstName} {newAdminLastName},",
            mainContent: $"""
                <p>{message}</p>
                <p>Un compte administrateur a été créé pour vous. Vous pouvez dès maintenant vous connecter à la plateforme.</p>
            """,
            buttonText: "Accéder à la plateforme",
            buttonLink: loginLink,
            footerNote: "Si vous n'êtes pas à l'origine de cette demande, veuillez contacter l'équipe support."
        );

        return await SendEmailSafelyAsync(receiverEmail, subject, htmlContent);
    }

    public async Task<Result> SendPasswordResetEmail(
        string resetToken,
        string firstName,
        string lastName,
        string email,
        string subject,
        string message,
        TimeSpan? linkExpiration = null)
    {
        var resetLink = $"{_baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";
        
        var htmlContent = BuildEmailTemplate(
            title: "Réinitialisation de mot de passe",
            greeting: $"Bonjour {firstName} {lastName},",
            mainContent: $"""
                <p>{message}</p>
                <p>Cliquez sur le bouton ci-dessous pour réinitialiser votre mot de passe :</p>
            """,
            buttonText: "Réinitialiser mon mot de passe",
            buttonLink: resetLink,
            footerNote: "Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.",
            linkExpiration: linkExpiration
        );

        return await SendEmailSafelyAsync(email, subject, htmlContent);
    }

    public async Task<Result> SendPasswordResetConfirmationEmail(
        string firstName,
        string lastName,
        string email,
        string subject,
        string message)
    {
        var loginLink = $"{_baseUrl}/login";
        
        var htmlContent = BuildEmailTemplate(
            title: "Mot de passe modifié",
            greeting: $"Bonjour {firstName} {lastName},",
            mainContent: $"""
                <p>{message}</p>
                <p>Votre mot de passe a été modifié avec succès. Vous pouvez maintenant vous connecter avec votre nouveau mot de passe.</p>
            """,
            buttonText: "Se connecter",
            buttonLink: loginLink,
            footerNote: "Si vous n'êtes pas à l'origine de ce changement, contactez immédiatement notre support.",
            showSecurityAlert: true
        );

        return await SendEmailSafelyAsync(email, subject, htmlContent);
    }

    private async Task<Result> SendEmailSafelyAsync(string email, string subject, string htmlContent)
    {
        try
        {
            await _emailSender.SendEmailAsync(email, subject, htmlContent);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to send email: {ex.Message}");
        }
    }

    private static string FormatExpiration(TimeSpan expiration)
    {
        if (expiration.TotalDays >= 1)
        {
            var days = (int)expiration.TotalDays;
            return days == 1 ? "1 jour" : $"{days} jours";
        }
        
        if (expiration.TotalHours >= 1)
        {
            var hours = (int)expiration.TotalHours;
            return hours == 1 ? "1 heure" : $"{hours} heures";
        }
        
        var minutes = (int)expiration.TotalMinutes;
        return minutes <= 1 ? "1 minute" : $"{minutes} minutes";
    }

    private static string BuildEmailTemplate(
        string title,
        string greeting,
        string mainContent,
        string buttonText,
        string buttonLink,
        string footerNote,
        TimeSpan? linkExpiration = null,
        bool showSecurityAlert = false)
    {
        var warningSection = linkExpiration.HasValue 
            ? $"""<p style="color: #e74c3c; font-size: 14px; margin-top: 20px;">⏰ Ce lien expire dans {FormatExpiration(linkExpiration.Value)}.</p>""" 
            : string.Empty;

        var securityAlertSection = showSecurityAlert
            ? """
                <div style="background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 8px; padding: 15px; margin-top: 20px;">
                    <p style="margin: 0; color: #856404; font-size: 14px;">
                        🔒 <strong>Alerte de sécurité :</strong> Si vous n'êtes pas à l'origine de cette action, 
                        veuillez sécuriser votre compte immédiatement.
                    </p>
                </div>
            """
            : string.Empty;

        return $"""
            <!DOCTYPE html>
            <html lang="fr">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{title}</title>
            </head>
            <body style="margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f7; line-height: 1.6;">
                <table role="presentation" style="width: 100%; border-collapse: collapse;">
                    <tr>
                        <td align="center" style="padding: 40px 20px;">
                            <table role="presentation" style="max-width: 600px; width: 100%; border-collapse: collapse; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);">
                                
                                <!-- Header -->
                                <tr>
                                    <td style="padding: 40px 40px 20px; text-align: center; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 12px 12px 0 0;">
                                        <h1 style="margin: 0; color: #ffffff; font-size: 24px; font-weight: 600;">{title}</h1>
                                    </td>
                                </tr>
                                
                                <!-- Body -->
                                <tr>
                                    <td style="padding: 40px;">
                                        <p style="margin: 0 0 20px; font-size: 16px; color: #333333; font-weight: 600;">{greeting}</p>
                                        
                                        <div style="color: #555555; font-size: 15px;">
                                            {mainContent}
                                        </div>
                                        
                                        <!-- CTA Button -->
                                        <table role="presentation" style="width: 100%; margin: 30px 0;">
                                            <tr>
                                                <td align="center">
                                                    <a href="{buttonLink}" 
                                                       style="display: inline-block; 
                                                              padding: 16px 32px; 
                                                              background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                                              color: #ffffff; 
                                                              text-decoration: none; 
                                                              font-size: 16px; 
                                                              font-weight: 600; 
                                                              border-radius: 8px;
                                                              box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);">
                                                        {buttonText}
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>
                                        
                                        {warningSection}
                                        {securityAlertSection}
                                        
                                        <!-- Fallback link -->
                                        <p style="margin: 25px 0 0; font-size: 13px; color: #888888;">
                                            Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur :<br>
                                            <a href="{buttonLink}" style="color: #667eea; word-break: break-all;">{buttonLink}</a>
                                        </p>
                                    </td>
                                </tr>
                                
                                <!-- Footer -->
                                <tr>
                                    <td style="padding: 30px 40px; background-color: #f8f9fa; border-radius: 0 0 12px 12px; border-top: 1px solid #e9ecef;">
                                        <p style="margin: 0 0 10px; font-size: 13px; color: #888888; text-align: center;">
                                            {footerNote}
                                        </p>
                                        <p style="margin: 0; font-size: 12px; color: #aaaaaa; text-align: center;">
                                            © {DateTime.UtcNow.Year} - Tous droits réservés
                                        </p>
                                    </td>
                                </tr>
                                
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}