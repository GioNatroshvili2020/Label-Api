using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class ReleaseController : ControllerBase
{
    private readonly IReleaseService _releaseService;

    public ReleaseController(IReleaseService releaseService)
    {
        _releaseService = releaseService;
    }

    [Authorize]
    [HttpPost("upload")]
    [RequestSizeLimit(110_000_000)] // 110MB
    public async Task<IActionResult> UploadRelease([FromForm] ReleaseUploadForm form)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var dto = new ReleaseUploadDto
        {
            ReleaseName = form.ReleaseName,
            ReleaseVersion = form.ReleaseVersion,
            FeaturingArtist = form.FeaturingArtist,
            PrimaryArtist = form.PrimaryArtist,
            Roles = form.Roles,
            Contributors = form.Contributors,
            Genre = form.Genre,
            Subgenre = form.Subgenre,
            TypeOfRelease = form.TypeOfRelease
        };

        var (success, error, release) = await _releaseService.UploadReleaseAsync(
            userId, dto, form.CoverArt, form.AudioFile);

        if (!success)
            return BadRequest(new { error });
        return Ok(release);
    }
} 