using MimeDetective;
using NAudio.Wave;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using label_api.Data;
using label_api.DTOs;
using label_api.Models;
using label_api.Options;
using label_api.Exceptions;
using System.Threading.Tasks;

public class ReleaseService : IReleaseService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ReleaseUploadValidator _validator;
    private readonly string _coverArtDir;
    private readonly string _audioDir;
    private readonly ReleaseUploadOptions _options;

    public ReleaseService(ApplicationDbContext dbContext, ReleaseUploadValidator validator, IOptions<ReleaseUploadOptions> options)
    {
        _dbContext = dbContext;
        _validator = validator;
        _options = options.Value;
        _coverArtDir = _options.CoverArtDir;
        _audioDir = _options.AudioDir;
        Directory.CreateDirectory(_coverArtDir);
        Directory.CreateDirectory(_audioDir);
    }

    public async Task<(bool Success, string ErrorMessage, ReleaseDto Release)> UploadReleaseAsync(string userId, ReleaseUploadDto dto, IFormFile coverArt, IFormFile audioFile)
    {
        // Validate cover art
        if (coverArt == null || coverArt.Length == 0)
            return (false, "Cover art is required.", null);
        if (coverArt.Length > _options.MaxCoverArtSize)
            return (false, "Cover art file is too large.", null);
        var coverArtExt = Path.GetExtension(coverArt.FileName).ToLowerInvariant();
        if (Array.IndexOf(_options.AllowedImageExtensions, coverArtExt) < 0)
            return (false, "Invalid cover art file type.", null);
        var (coverValid, coverError) = _validator.ValidateCoverArt(coverArt, _options.AllowedImageExtensions, _options.MaxCoverArtSize);
        if (!coverValid)
            return (false, coverError, null);

        // Validate audio file
        if (audioFile == null || audioFile.Length == 0)
            return (false, "Audio file is required.", null);
        if (audioFile.Length > _options.MaxAudioSize)
            return (false, "Audio file is too large.", null);
        var audioExt = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
        if (Array.IndexOf(_options.AllowedAudioExtensions, audioExt) < 0)
            return (false, "Invalid audio file type.", null);
        var (audioValid, audioError) = _validator.ValidateAudioFile(audioFile, _options.AllowedAudioExtensions, _options.MaxAudioSize);
        if (!audioValid)
            return (false, audioError, null);

        // Save files
        var coverArtFileName = $"{Guid.NewGuid()}{coverArtExt}";
        var audioFileName = $"{Guid.NewGuid()}{audioExt}";
        var coverArtPath = Path.Combine(_coverArtDir, coverArtFileName);
        var audioPath = Path.Combine(_audioDir, audioFileName);
        try
        {
            using (var stream = new FileStream(coverArtPath, FileMode.Create))
            {
                await coverArt.CopyToAsync(stream);
            }
            using (var stream = new FileStream(audioPath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Save to DB
            var release = new Release
            {
                UserId = userId,
                CoverArtPath = coverArtPath,
                AudioFilePath = audioPath,
                ReleaseName = dto.ReleaseName,
                ReleaseVersion = dto.ReleaseVersion,
                FeaturingArtist = dto.FeaturingArtist,
                PrimaryArtist = dto.PrimaryArtist,
                Roles = dto.Roles,
                Contributors = dto.Contributors,
                Genre = dto.Genre,
                Subgenre = dto.Subgenre,
                TypeOfRelease = dto.TypeOfRelease,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Add(release);
            await _dbContext.SaveChangesAsync();

            var result = new ReleaseDto
            {
                Id = release.Id,
                ReleaseName = release.ReleaseName,
                ReleaseVersion = release.ReleaseVersion,
                FeaturingArtist = release.FeaturingArtist,
                PrimaryArtist = release.PrimaryArtist,
                Roles = release.Roles,
                Contributors = release.Contributors,
                Genre = release.Genre,
                Subgenre = release.Subgenre,
                TypeOfRelease = release.TypeOfRelease,
                CoverArtPath = GetMediaUrl(coverArtPath),
                AudioFilePath = GetMediaUrl(audioPath)
            };
            return (true, null, result);
        }
        catch (Exception ex)
        {
            // Clean up files if DB save fails or any error occurs
            if (File.Exists(coverArtPath))
                File.Delete(coverArtPath);
            if (File.Exists(audioPath))
                File.Delete(audioPath);
            return (false, $"Failed to save release: {ex.Message}", null);
        }
    }

    public async Task<IEnumerable<ReleaseDto>> GetUserReleasesAsync(string userId)
    {
        var releases = await _dbContext.Releases
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReleaseDto
            {
                Id = r.Id,
                ReleaseName = r.ReleaseName,
                ReleaseVersion = r.ReleaseVersion,
                FeaturingArtist = r.FeaturingArtist,
                PrimaryArtist = r.PrimaryArtist,
                Roles = r.Roles,
                Contributors = r.Contributors,
                Genre = r.Genre,
                Subgenre = r.Subgenre,
                TypeOfRelease = r.TypeOfRelease,
                CoverArtPath = r.CoverArtPath,
                AudioFilePath = r.AudioFilePath
            })
            .ToListAsync();

        // Convert file paths to URLs
        foreach (var release in releases)
        {
            release.CoverArtPath = GetMediaUrl(release.CoverArtPath);
            release.AudioFilePath = GetMediaUrl(release.AudioFilePath);
        }

        return releases;
    }

    public async Task<IEnumerable<ReleaseDto>> SearchUserReleasesAsync(string userId, ReleaseSearchDto searchDto)
    {
        var query = _dbContext.Releases.Where(r => r.UserId == userId);

        // Apply search filters
        if (!string.IsNullOrEmpty(searchDto.ReleaseName))
            query = query.Where(r => r.ReleaseName.Contains(searchDto.ReleaseName));

        if (!string.IsNullOrEmpty(searchDto.PrimaryArtist))
            query = query.Where(r => r.PrimaryArtist.Contains(searchDto.PrimaryArtist));

        if (!string.IsNullOrEmpty(searchDto.FeaturingArtist))
            query = query.Where(r => r.FeaturingArtist != null && r.FeaturingArtist.Contains(searchDto.FeaturingArtist));

        if (!string.IsNullOrEmpty(searchDto.Genre))
            query = query.Where(r => r.Genre != null && r.Genre.Contains(searchDto.Genre));

        if (!string.IsNullOrEmpty(searchDto.Subgenre))
            query = query.Where(r => r.Subgenre != null && r.Subgenre.Contains(searchDto.Subgenre));

        if (!string.IsNullOrEmpty(searchDto.TypeOfRelease))
            query = query.Where(r => r.TypeOfRelease != null && r.TypeOfRelease.Contains(searchDto.TypeOfRelease));

        if (!string.IsNullOrEmpty(searchDto.Contributors))
            query = query.Where(r => r.Contributors != null && r.Contributors.Contains(searchDto.Contributors));

        if (searchDto.CreatedAfter.HasValue)
            query = query.Where(r => r.CreatedAt >= searchDto.CreatedAfter.Value);

        if (searchDto.CreatedBefore.HasValue)
            query = query.Where(r => r.CreatedAt <= searchDto.CreatedBefore.Value);

        var releases = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReleaseDto
            {
                Id = r.Id,
                ReleaseName = r.ReleaseName,
                ReleaseVersion = r.ReleaseVersion,
                FeaturingArtist = r.FeaturingArtist,
                PrimaryArtist = r.PrimaryArtist,
                Roles = r.Roles,
                Contributors = r.Contributors,
                Genre = r.Genre,
                Subgenre = r.Subgenre,
                TypeOfRelease = r.TypeOfRelease,
                CoverArtPath = r.CoverArtPath,
                AudioFilePath = r.AudioFilePath
            })
            .ToListAsync();

        // Convert file paths to URLs
        foreach (var release in releases)
        {
            release.CoverArtPath = GetMediaUrl(release.CoverArtPath);
            release.AudioFilePath = GetMediaUrl(release.AudioFilePath);
        }

        return releases;
    }

    public async Task<bool> UpdateReleaseAsync(string userId, int releaseId, UpdateReleaseDto dto)
    {
        var release = await _dbContext.Releases.FindAsync(releaseId);
        if (release == null || release.UserId != userId)
            throw LabelApiException.BadRequest("Label does not exist, or you do not have permission to update it.");
        if (release.LabelStatus == (int)ReleaseStatus.Approved || release.LabelStatus == (int)ReleaseStatus.Rejected)
            throw LabelApiException.BadRequest("Cannot Update Approved or Rejected Labels");

        // Handle CoverArt
        if (dto.CoverArt != null && dto.CoverArt.Length > 0)
        {
            var coverArtExt = Path.GetExtension(dto.CoverArt.FileName).ToLowerInvariant();
            if (Array.IndexOf(_options.AllowedImageExtensions, coverArtExt) < 0)
                throw LabelApiException.BadRequest("Invalid cover art file type.");
            var (coverValid, coverError) = _validator.ValidateCoverArt(dto.CoverArt, _options.AllowedImageExtensions, _options.MaxCoverArtSize);
            if (!coverValid)
                throw LabelApiException.BadRequest(coverError);
            var coverArtFileName = $"{Guid.NewGuid()}{coverArtExt}";
            var coverArtPath = Path.Combine(_coverArtDir, coverArtFileName);
            using (var stream = new FileStream(coverArtPath, FileMode.Create))
            {
                await dto.CoverArt.CopyToAsync(stream);
            }
            release.CoverArtPath = coverArtPath;
        }
        // Handle AudioFile
        if (dto.AudioFile != null && dto.AudioFile.Length > 0)
        {
            var audioExt = Path.GetExtension(dto.AudioFile.FileName).ToLowerInvariant();
            if (Array.IndexOf(_options.AllowedAudioExtensions, audioExt) < 0)
                throw LabelApiException.BadRequest("Invalid audio file type.");
            var (audioValid, audioError) = _validator.ValidateAudioFile(dto.AudioFile, _options.AllowedAudioExtensions, _options.MaxAudioSize);
            if (!audioValid)
                throw LabelApiException.BadRequest(audioError);
            var audioFileName = $"{Guid.NewGuid()}{audioExt}";
            var audioPath = Path.Combine(_audioDir, audioFileName);
            using (var stream = new FileStream(audioPath, FileMode.Create))
            {
                await dto.AudioFile.CopyToAsync(stream);
            }
            release.AudioFilePath = audioPath;
        }
        // Update other fields
        release.ReleaseName = dto.ReleaseName;
        release.ReleaseVersion = dto.ReleaseVersion;
        release.FeaturingArtist = dto.FeaturingArtist;
        release.PrimaryArtist = dto.PrimaryArtist;
        release.Roles = dto.Roles;
        release.Contributors = dto.Contributors;
        release.Genre = dto.Genre;
        release.Subgenre = dto.Subgenre;
        release.TypeOfRelease = dto.TypeOfRelease;
        release.UpdatedAt = System.DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ReleaseDto>> AdminSearchReleasesAsync(ReleaseSearchDto searchDto)
    {
        var query = _dbContext.Releases.AsQueryable();

        // Apply search filters (same as SearchUserReleasesAsync)
        if (!string.IsNullOrEmpty(searchDto.ReleaseName))
            query = query.Where(r => r.ReleaseName.Contains(searchDto.ReleaseName));
        if (!string.IsNullOrEmpty(searchDto.PrimaryArtist))
            query = query.Where(r => r.PrimaryArtist.Contains(searchDto.PrimaryArtist));
        if (!string.IsNullOrEmpty(searchDto.FeaturingArtist))
            query = query.Where(r => r.FeaturingArtist != null && r.FeaturingArtist.Contains(searchDto.FeaturingArtist));
        if (!string.IsNullOrEmpty(searchDto.Genre))
            query = query.Where(r => r.Genre != null && r.Genre.Contains(searchDto.Genre));
        if (!string.IsNullOrEmpty(searchDto.Subgenre))
            query = query.Where(r => r.Subgenre != null && r.Subgenre.Contains(searchDto.Subgenre));
        if (!string.IsNullOrEmpty(searchDto.TypeOfRelease))
            query = query.Where(r => r.TypeOfRelease != null && r.TypeOfRelease.Contains(searchDto.TypeOfRelease));
        if (!string.IsNullOrEmpty(searchDto.Contributors))
            query = query.Where(r => r.Contributors != null && r.Contributors.Contains(searchDto.Contributors));
        if (searchDto.CreatedAfter.HasValue)
            query = query.Where(r => r.CreatedAt >= searchDto.CreatedAfter.Value);
        if (searchDto.CreatedBefore.HasValue)
            query = query.Where(r => r.CreatedAt <= searchDto.CreatedBefore.Value);

        var releases = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReleaseDto
            {
                Id = r.Id,
                ReleaseName = r.ReleaseName,
                ReleaseVersion = r.ReleaseVersion,
                FeaturingArtist = r.FeaturingArtist,
                PrimaryArtist = r.PrimaryArtist,
                Roles = r.Roles,
                Contributors = r.Contributors,
                Genre = r.Genre,
                Subgenre = r.Subgenre,
                TypeOfRelease = r.TypeOfRelease,
                CoverArtPath = r.CoverArtPath,
                AudioFilePath = r.AudioFilePath
            })
            .ToListAsync();

        // Convert file paths to URLs
        foreach (var release in releases)
        {
            release.CoverArtPath = GetMediaUrl(release.CoverArtPath);
            release.AudioFilePath = GetMediaUrl(release.AudioFilePath);
        }

        return releases;
    }

    public async Task<PagedResult<ReleaseDto>> GetAllReleasesPagedAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var query = _dbContext.Releases.AsQueryable();
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var releases = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReleaseDto
            {
                Id = r.Id,
                ReleaseName = r.ReleaseName,
                ReleaseVersion = r.ReleaseVersion,
                FeaturingArtist = r.FeaturingArtist,
                PrimaryArtist = r.PrimaryArtist,
                Roles = r.Roles,
                Contributors = r.Contributors,
                Genre = r.Genre,
                Subgenre = r.Subgenre,
                TypeOfRelease = r.TypeOfRelease,
                CoverArtPath = r.CoverArtPath,
                AudioFilePath = r.AudioFilePath
            })
            .ToListAsync();
        foreach (var release in releases)
        {
            release.CoverArtPath = GetMediaUrl(release.CoverArtPath);
            release.AudioFilePath = GetMediaUrl(release.AudioFilePath);
        }
        return new PagedResult<ReleaseDto>
        {
            Items = releases,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task UpdateReleaseStatusAsync(StatusUpdateDto dto)
    {
        var release = await _dbContext.Releases.FindAsync(dto.ReleaseId);
        if (release == null)
            throw LabelApiException.BadRequest("Release not found");
        release.LabelStatus = (int)dto.Status;
        if (!string.IsNullOrEmpty(dto.RejectReason))
            release.RejectMessage = dto.RejectReason;
        await _dbContext.SaveChangesAsync();
    }

    private string GetMediaUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return filePath;

        // Convert file path to URL path
        var relativePath = Path.GetRelativePath("uploads", filePath).Replace('\\', '/');
        return $"{_options.MediaBaseUrl}/{relativePath}";
    }
}