using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Presentation.Controllers;

[Route("api/tokens")]
[ApiController]
public class TokenController : Controller
{
    private readonly ITokenService _tokenService;
    public TokenController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] RefreshDto model)
    {
        var response = await _tokenService.Refresh(model);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke()
    {
        var userEmail = HttpContext.User.Identity?.Name;
        await _tokenService.RevokeRefreshTokenByEmail(userEmail);
        return Ok();
    }

}
