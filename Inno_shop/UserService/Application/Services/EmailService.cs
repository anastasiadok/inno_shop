using MailKit.Net.Smtp;
using MimeKit;
using UserService.Application.Interfaces;
using UserService.Domain.Exceptions;
using UserService.Domain.Models;
using UserService.Infrastructure.Interfaces;

namespace UserService.Application.Services;

public class EmailService : IEmailService
{
    private readonly IUserRepository _repository;
    private readonly IConfiguration _config;

    public EmailService(IUserRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _config = configuration;
    }

    public async Task SendConfirmEmail(string email, string emailBodyUrl)
    {
        var subject = "Email confirmation";
        var emailBody = $"To confirm your email <a href=\"{emailBodyUrl}\">click here </a> ";
        await SendEmail(email, subject, emailBody);
    }

    public async Task SendResetPasswordEmail(string email, string emailBodyUrl)
    {
        var subject = "Password reset";
        var emailBody = $"To reset your password <a href=\"{emailBodyUrl}\">click here </a> ";
        await SendEmail(email, subject, emailBody);
    }

    private async Task SendEmail(string email, string subject, string message)
    {
        using var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_config["Email:From:Name"], _config["Email:From:Address"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };
        
        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_config["Email:SmtpServer"], Convert.ToInt32(_config["Email:Port"]), Convert.ToBoolean(_config["Email:UseSSL"]));
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            var res1 = await client.SendAsync(emailMessage);
        }
        catch
        {
            throw new Exception("Email sender trouble");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    public async Task<bool> CheckEmailIsFree(string email)
    {
        var emailUser = await _repository.GetByEmailAsync(email);
        return emailUser is null;
    }

    public async Task ConfirmEmailByToken(string email, string token)
    {
        var user = await _repository.GetByEmailAsync(email) ?? throw new NotFoundException(nameof(User));

        if (user.IsEmailConfirmed)
            throw new BadRequestException("Email is already confirmed");
       
        if (user.EmailConfirmToken != token)
            throw new BadRequestException("Invalid token");

        user.IsEmailConfirmed = true;
        await _repository.UpdateAsync(user);
    }
}
