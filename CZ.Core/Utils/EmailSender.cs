using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace api.CZ.Core.Utils;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? throw new KeyNotFoundException("No smtp host");
        var sender = Environment.GetEnvironmentVariable("SMTP_EMAIL_SENDER") ?? throw new KeyNotFoundException("No smtp email sender");
        var senderPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? throw new KeyNotFoundException("No smtp password");

        var client = new SmtpClient()
        {
            Port = 587,
            UseDefaultCredentials = false,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Host = host,
            Credentials = new NetworkCredential(sender, senderPassword)
        };
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(sender),
            Subject = subject,
            Body = message,
            IsBodyHtml = true 
        };
        mailMessage.To.Add(email);
        
        return client.SendMailAsync(mailMessage);
    }
}