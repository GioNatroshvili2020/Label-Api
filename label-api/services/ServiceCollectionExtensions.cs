using MimeDetective;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
}