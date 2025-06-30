using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IReleaseService
{
    Task<(bool Success, string ErrorMessage, ReleaseDto Release)> UploadReleaseAsync(string userId, ReleaseUploadDto dto, IFormFile coverArt, IFormFile audioFile);
} 