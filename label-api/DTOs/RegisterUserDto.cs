namespace label_api.DTOs;

public class RegisterUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public string ArtistName { get; set; }
    public List<string> ExistingDspProfileLinks { get; set; }
} 