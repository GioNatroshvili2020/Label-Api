public class ReleaseUploadOptions
{
    public long MaxCoverArtSize { get; set; } = 5 * 1024 * 1024; // 5MB default
    public long MaxAudioSize { get; set; } = 100 * 1024 * 1024; // 100MB default
    public string[] AllowedImageExtensions { get; set; } = new[] { ".jpg", ".jpeg", ".png" };
    public string[] AllowedAudioExtensions { get; set; } = new[] { ".mp3", ".wav", ".flac" };
    public string CoverArtDir { get; set; } = "uploads/coverart";
    public string AudioDir { get; set; } = "uploads/audio";
} 