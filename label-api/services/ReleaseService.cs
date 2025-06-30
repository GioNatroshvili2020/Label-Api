using MimeDetective;
using NAudio.Wave;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Options;

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
                CoverArtPath = coverArtPath,
                AudioFilePath = audioPath
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
}