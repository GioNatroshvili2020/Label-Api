using Microsoft.AspNetCore.Identity;
using label_api.DTOs;
using label_api.Models;
using System.Threading.Tasks;
using System.Security.Claims;

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

    public async Task<(SignInResult Result, UserInfoDto UserInfo)> LoginAsync(LoginUserDto dto)
    {
        var signInResult = await _signInManager.PasswordSignInAsync(dto.Email, dto.Password, false, false);
        if (!signInResult.Succeeded)
            return (signInResult, null);
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return (signInResult, null);
        var userInfo = new UserInfoDto
        {
            Username = user.Username,
            Email = user.Email,
            ArtistName = user.ArtistName,
            ExistingDspProfileLinks = user.ExistingDspProfileLinks
        };
        return (signInResult, userInfo);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<UserInfoDto> GetUserInfoByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        return new UserInfoDto
        {
            Username = user.Username,
            Email = user.Email,
            ArtistName = user.ArtistName,
            ExistingDspProfileLinks = user.ExistingDspProfileLinks
        };
    }
}