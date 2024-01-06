using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos;
using UserService.Application.Interfaces;

namespace UserService.Presentation.Controllers;

[Authorize]
[Route("api/users")]
[ApiController]
public class UserController : Controller
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetAll()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById([FromRoute] Guid id)
    {
        var user = await _userService.GetById(id);
        return Ok(user);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto user)
    {
        await _userService.Update(user);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {   
        await _userService.DeleteUserProducts(id, Request.Headers["Authorization"]);
        await _userService.DeleteById(id);
        return Ok();
    }
}
