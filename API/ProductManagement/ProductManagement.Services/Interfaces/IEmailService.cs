using ProductManagement.EFCore.IdentityModels;

namespace ProductManagement.Services.Interfaces;
public interface IEmailService
{
    Task SendEmail(Email email);
    Task SendEmailFromUser(EmailFromUser email);
}
