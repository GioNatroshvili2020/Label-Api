using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace label_api.DTOs;

public class ReleaseUploadForm
{
    [FromForm] public string ReleaseName { get; set; }
    [FromForm] public string ReleaseVersion { get; set; }
    [FromForm] public string FeaturingArtist { get; set; }
    [FromForm] public string PrimaryArtist { get; set; }
    [FromForm] public string Roles { get; set; }
    [FromForm] public string Contributors { get; set; }
    [FromForm] public string Genre { get; set; }
    [FromForm] public string Subgenre { get; set; }
    [FromForm] public string TypeOfRelease { get; set; }
    [FromForm] public IFormFile CoverArt { get; set; }
    [FromForm] public IFormFile AudioFile { get; set; }
} 