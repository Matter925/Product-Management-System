using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

using SendGrid;
using SendGrid.Helpers.Mail;

using System.Net.Security;
using System.Security.Authentication;

using ProductManagement.EFCore.IdentityModels;
using ProductManagement.Services.Interfaces;

namespace ProductManagement.Services.Implementation;

public class EmailService : IEmailService
{
    private readonly EFCore.IdentityModels.MailSettings _options;
    public EmailService(IOptions<EFCore.IdentityModels.MailSettings> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Sends an email using the specified email details.
    /// </summary>
    /// <param name="email">The email details, including recipients, subject, and body.</param>
    /// <exception cref="Exception">Thrown if there is a failure in sending the email.</exception>
    /// <returns>A task representing the asynchronous operation of sending the email.</returns>
    public async Task SendEmail(Email email)
    {
        var mail = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_options.EmailFrom),
            Subject = email.Subject,
        };
        mail.To.Add(MailboxAddress.Parse(email.To));
        var builder = new BodyBuilder
        {
            HtmlBody = email.Body
        };
        mail.Body = builder.ToMessageBody();
        mail.From.Add(new MailboxAddress(_options.DisplayName, _options.EmailFrom));

        if (_options.EmailFor == 2)
        {
            // Send using SMTP
            using var smtp = new SmtpClient();
            try
            {
                smtp.CheckCertificateRevocation = true; // Enable certificate revocation check
                smtp.SslProtocols = SslProtocols.Tls12; // Enforce TLS 1.2
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => e == SslPolicyErrors.None;
                await smtp.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
                await smtp.SendAsync(mail);
                await smtp.DisconnectAsync(true);
            }
            catch
            {
                throw new Exception($"FailedToSendEmail");
            }
        }
    }

    /// <summary>
    /// Sends an email using the specified details provided by a user.
    /// </summary>
    /// <param name="email">The email details, including sender, recipients, subject, and body.</param>
    /// <exception cref="Exception">Thrown if there is a failure in sending the email.</exception>
    /// <returns>A task representing the asynchronous operation of sending the email.</returns>
    public async Task SendEmailFromUser(EmailFromUser email)
    {
        var mail = new MimeMessage
        {
            Sender = MailboxAddress.Parse(_options.EmailFrom),
        };
        mail.To.Add(MailboxAddress.Parse(_options.EmailTo));
        var builder = new BodyBuilder
        {
            HtmlBody = email.Body
        };
        mail.ReplyTo.Add(MailboxAddress.Parse(email.ReplyTo));
        mail.Body = builder.ToMessageBody();
        mail.From.Add(new MailboxAddress(_options.DisplayName, _options.EmailFrom));

        if (_options.EmailFor == 1)
        {
            // Send using SendGrid
            var client = new SendGridClient(_options.ApiKey);
            var from = new EmailAddress(_options.EmailFrom, _options.DisplayName);
            var to = new EmailAddress(_options.EmailTo);
            var plainTextContent = email.Body;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, "ContactUs", plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
        else if (_options.EmailFor == 2)
        {
            // Send using SMTP
            using var smtp = new SmtpClient();
            try
            {
                smtp.CheckCertificateRevocation = true; // Enable certificate revocation check
                smtp.SslProtocols = SslProtocols.Tls12; // Enforce TLS 1.2
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => e == SslPolicyErrors.None;
                await smtp.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
                await smtp.SendAsync(mail);
                await smtp.DisconnectAsync(true);
            }
            catch
            {
                throw new Exception($"FailedToSendEmail");
            }
        }
    }
}
