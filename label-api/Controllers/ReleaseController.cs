using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using label_api.Exceptions;
using label_api.DTOs;

[ApiController]
[Route("api/releases")]
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
            throw LabelApiException.BadRequest(error ?? "Failed to upload release");
        
        return Ok(release);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetReleases()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);   
        var releases = await _releaseService.GetUserReleasesAsync(userId);
        return Ok(releases);
    }

    [Authorize]
    [HttpGet("search")]
    public async Task<IActionResult> SearchReleases([FromQuery] ReleaseSearchDto searchDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var releases = await _releaseService.SearchUserReleasesAsync(userId, searchDto);
        return Ok(releases);
    }

    [Authorize]
    [HttpPut("{releaseId}")]
    public async Task<IActionResult> UpdateLabel(int releaseId, [FromForm] UpdateReleaseDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);      
        var updated = await _releaseService.UpdateReleaseAsync(userId, releaseId, dto);
        return Ok("Label Succesfully Updated");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/search")]
    public async Task<IActionResult> AdminSearchReleases([FromQuery] ReleaseSearchDto searchDto)
    {
        var releases = await _releaseService.AdminSearchReleasesAsync(searchDto);
        return Ok(releases);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllReleases([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _releaseService.GetAllReleasesPagedAsync(page, pageSize);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("admin/status")]
    public async Task<IActionResult> UpdateReleaseStatus([FromBody] StatusUpdateDto dto)
    {
        await _releaseService.UpdateReleaseStatusAsync(dto);
        return Ok("Status updated");
    }
} 