using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using label_api.Models;
using label_api.Options;

public static class AdminSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var options = serviceProvider.GetRequiredService<IOptions<AdminUserOptions>>();
        var adminConfig = options.Value;

        const string adminRole = "Admin";
        var adminEmail = adminConfig.Email;
        var adminPassword = adminConfig.Password;

        // Ensure the Admin role exists
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        // Check if the admin user exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Email = adminEmail,
                UserName = adminEmail,
                Username = "admin",
                ArtistName = "Admin",
                ExistingDspProfileLinks = new List<string>()
            };
            await userManager.CreateAsync(adminUser, adminPassword);
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}