using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using label_api.DTOs;

public interface IReleaseService
{
    Task<(bool Success, string ErrorMessage, ReleaseDto Release)> UploadReleaseAsync(string userId, ReleaseUploadDto dto, IFormFile coverArt, IFormFile audioFile);
    Task<IEnumerable<ReleaseDto>> GetUserReleasesAsync(string userId);
    Task<IEnumerable<ReleaseDto>> SearchUserReleasesAsync(string userId, ReleaseSearchDto searchDto);
    Task<bool> UpdateReleaseAsync(string userId, int releaseId, UpdateReleaseDto dto);
    Task<IEnumerable<ReleaseDto>> AdminSearchReleasesAsync(ReleaseSearchDto searchDto);
    Task<PagedResult<ReleaseDto>> GetAllReleasesPagedAsync(int page, int pageSize);
    Task UpdateReleaseStatusAsync(StatusUpdateDto dto);
} 