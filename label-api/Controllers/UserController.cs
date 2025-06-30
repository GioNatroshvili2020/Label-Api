using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using label_api.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var result = await _userService.RegisterAsync(dto);
        if (result.Succeeded)
            return Ok();
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        var (result, userInfo) = await _userService.LoginAsync(dto);
        if (result.Succeeded)
            return Ok(userInfo);
        return Unauthorized();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _userService.LogoutAsync();
        return Ok();
    }

    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var userInfo = await _userService.GetUserInfoByIdAsync(userId);
        if (userInfo == null)
            return NotFound();
        return Ok(userInfo);
    }
} 