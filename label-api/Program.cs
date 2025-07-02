using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using label_api.Data;
using label_api.Models;
using label_api.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCustomServices(builder.Configuration);
builder.Services.AddGlobalErrorHandling();
builder.Services.AddDevelopmentCors(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403;
        return Task.CompletedTask;
    };
    
    // Create persistent cookie and configure SameSite for development
    options.Events.OnSigningIn = context =>
    {
        context.Properties.IsPersistent = true;

        // Disabling same-site policy in case of local environment
        if (builder.Environment.IsDevelopment())
        {
            context.CookieOptions.SameSite = SameSiteMode.None;
            context.CookieOptions.Secure = true;
        }
        return Task.CompletedTask;
    };
    
    // Additional SameSite configuration for development
    if (builder.Environment.IsDevelopment())
    {
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
});

builder.Services.Configure<AdminUserOptions>(builder.Configuration.GetSection("AdminUser"));

// Configure media base URL from environment
builder.Services.PostConfigure<ReleaseUploadOptions>(options =>
{
    var mediaBaseUrl = builder.Configuration.GetValue<string>("MediaBaseUrl");
    if (!string.IsNullOrEmpty(mediaBaseUrl))
    {
        options.MediaBaseUrl = mediaBaseUrl;
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Add global exception handling first
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed admin user and role
using (var scope = app.Services.CreateScope())
{
    AdminSeeder.SeedAdminAsync(scope.ServiceProvider).GetAwaiter().GetResult();
}

app.Run();
