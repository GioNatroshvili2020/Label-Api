using MimeDetective;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using label_api.Handlers;
using label_api.Options;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<UserService>();
        services.AddScoped<IReleaseService, ReleaseService>();
        services.AddScoped<ReleaseUploadValidator>();
        services.AddSingleton(provider =>
            new ContentInspectorBuilder
            {
                Definitions = MimeDetective.Definitions.DefaultDefinitions.All()
            }.Build()
        );
        services.Configure<ReleaseUploadOptions>(configuration.GetSection("ReleaseUpload"));
        return services;
    }

    public static IServiceCollection AddGlobalErrorHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        // Add problem details support for consistent error responses
        services.AddProblemDetails();
        
        return services;
    }

    public static IServiceCollection AddDevelopmentCors(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }
        return services;
    }
}