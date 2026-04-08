using api.CZ.Core.ResultPattern;

namespace api.CZ.Core.Services;

public interface IEmailService
{
    public Task<Result> SendRegisteringConfirmationEmail(string confirmationToken, string firstName, string lastName, string receiverEmail, string subject, string message, TimeSpan? linkExpiration = null);
    public Task<Result> SendAdministratorCreationEmail(string newAdminFirstName, string newAdminLastName, string receiverEmail, string confirmationToken, string subject, string message);
    public Task<Result> SendPasswordResetEmail(string resetToken,string firstName, string lastName, string email, string subject, string message, TimeSpan? linkExpiration = null);
    public Task<Result> SendPasswordResetConfirmationEmail(string firstName, string lastName, string email, string subject, string message);
}