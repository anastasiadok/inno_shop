using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Presentation.Controllers;

[AllowAnonymous]
[Route("api/auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public AuthController(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
    {
        var token = await _authService.Register(userDto);
        var emailBodyUrl = Request.Scheme + "://" + Request.Host+Url.Action("confirmemail", "auth",new { email = userDto.Email, token });
        await _emailService.SendConfirmEmail(userDto.Email, emailBodyUrl);
        return Ok("Registration was successful. Check email.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginModel)
    {
        var response = await _authService.Login(loginModel);
        return Ok(response);
    }

    [HttpGet("confirmemail")]
    public async Task<IActionResult> ConfirmEmail([EmailAddress] string email, string token)
    {
        await _emailService.ConfirmEmailByToken(email, token);
        return Ok("Confirmation was successful.");
    }

    [HttpPost("forgotpassword")]
    public async Task<IActionResult> ForgotPassword([EmailAddress] string email)
    {
        var token = await _authService.ForgotPassword(email);
        var emailBodyUrl = Request.Scheme + "://" + Request.Host + Url.Action("resetpassword", "auth", new { email, token });
        await _emailService.SendResetPasswordEmail(email, emailBodyUrl);
        return Ok($"Check {email} email. You may now reset your password whithin 1 hour.");
    }

    [HttpGet("resetpassword")]
    public ActionResult<ResetPasswordDto> ResetPassword([EmailAddress] string email, string token)
    {
        var model = new ResetPasswordDto(email, "", token);
        return Ok(model);
    }

    [HttpPost("resetpassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        await _authService.ResetPassword(model);
        return Ok("Password has been successfully changed.");
    }
}
