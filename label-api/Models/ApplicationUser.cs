using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace label_api.Models;

public class ApplicationUser : IdentityUser
{
    public string Username { get; set; }
    public string ArtistName { get; set; }
    public List<string> ExistingDspProfileLinks { get; set; } = new List<string>();
} 