namespace label_api.DTOs;

using System.Collections.Generic;

public class UserInfoDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string ArtistName { get; set; }
    public List<string> ExistingDspProfileLinks { get; set; }
} 