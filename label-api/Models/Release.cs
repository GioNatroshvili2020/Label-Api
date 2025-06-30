using System;

namespace label_api.Models;

public class Release
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string CoverArtPath { get; set; }
    public string AudioFilePath { get; set; }
    public string ReleaseName { get; set; }
    public string ReleaseVersion { get; set; }
    public string FeaturingArtist { get; set; }
    public string PrimaryArtist { get; set; }
    public string Roles { get; set; }
    public string Contributors { get; set; }
    public string Genre { get; set; }
    public string Subgenre { get; set; }
    public string TypeOfRelease { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 