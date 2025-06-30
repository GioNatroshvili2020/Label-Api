using Microsoft.AspNetCore.Identity;

public class RegisterUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
    public string ArtistName { get; set; }
    public List<string> ExistingDspProfileLinks { get; set; }
}

public class LoginUserDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterUserDto dto)
    {
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            Username = dto.Username,
            ArtistName = dto.ArtistName,
            ExistingDspProfileLinks = dto.ExistingDspProfileLinks ?? []
        };
        return await _userManager.CreateAsync(user, dto.Password);
    }

    public async Task<SignInResult> LoginAsync(LoginUserDto dto)
    {
        return await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}