namespace BcGov.Jag.AccountManagement.Server.Services;

public static class Extensions
{
    public static IServiceCollection AddLdapUserSearch(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .Configure<LdapConfiguration>(configuration.GetSection(LdapConfiguration.Section))
            .AddScoped<IUserSearchService, LdapUserSearchService>();

        return services;
    }
}
