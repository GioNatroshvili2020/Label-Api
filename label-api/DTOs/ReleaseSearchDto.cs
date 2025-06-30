namespace label_api.DTOs;

public class ReleaseSearchDto
{
    public string? ReleaseName { get; set; }
    public string? PrimaryArtist { get; set; }
    public string? FeaturingArtist { get; set; }
    public string? Genre { get; set; }
    public string? Subgenre { get; set; }
    public string? TypeOfRelease { get; set; }
    public string? Contributors { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
} 