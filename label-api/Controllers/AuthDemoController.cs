using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthDemoController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok("This is a public endpoint.");
    }

    [Authorize]
    [HttpGet("authenticated")]
    public IActionResult AuthenticatedEndpoint()
    {
        return Ok("This is an endpoint for authenticated users.");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminEndpoint()
    {
        return Ok("This is an endpoint for admin users only.");
    }
} 