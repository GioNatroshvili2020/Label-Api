using Microsoft.AspNetCore.Http;
using MimeDetective;
using System.IO;
using System.Linq;

public class ReleaseUploadValidator
{
    private readonly IContentInspector _contentInspector;
    public ReleaseUploadValidator(IContentInspector contentInspector)
    {
        _contentInspector = contentInspector;
    }

    public (bool IsValid, string Error) ValidateCoverArt(IFormFile coverArt, string[] allowedExtensions, long maxSize)
    {
        if (coverArt == null || coverArt.Length == 0)
            return (false, "Cover art is required.");
        if (coverArt.Length > maxSize)
            return (false, "Cover art file is too large.");
        var ext = Path.GetExtension(coverArt.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return (false, "Invalid cover art file type.");

        using (var stream = coverArt.OpenReadStream())
        {
            var result = _contentInspector.Inspect(stream);
            var mimeType = result.ByMimeType().FirstOrDefault()?.MimeType;
            if (mimeType == null || !mimeType.StartsWith("image/"))
                return (false, "Cover art file is not a valid image.");
        }
        return (true, null);
    }

    public (bool IsValid, string Error) ValidateAudioFile(IFormFile audioFile, string[] allowedExtensions, long maxSize)
    {
        if (audioFile == null || audioFile.Length == 0)
            return (false, "Audio file is required.");
        if (audioFile.Length > maxSize)
            return (false, "Audio file is too large.");
        var ext = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return (false, "Invalid audio file type.");

        using (var stream = audioFile.OpenReadStream())
        {
            var result = _contentInspector.Inspect(stream);
            var mimeType = result.ByMimeType().FirstOrDefault()?.MimeType;
            if (mimeType == null || !mimeType.StartsWith("audio/"))
                return (false, "Audio file is not a valid audio file.");
        }
        return (true, null);
    }
} 