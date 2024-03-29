﻿namespace UserService.Application.Interfaces;

public interface IEmailService
{
    Task SendConfirmEmail(string email, string emailBodyUrl);
    Task SendResetPasswordEmail(string email, string emailBodyUrl);
    Task ConfirmEmailByToken(string email, string token);
}
