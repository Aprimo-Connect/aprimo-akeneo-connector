using API.Configuration;
using API.Multitenancy;

namespace API.Akeneo
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddAkeneo(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddHttpClient<IAkeneoService, AkeneoService>();
			services.AddFileSystemTokenStorage((options) =>
			{
				options.Path = Path.Combine(AppContext.BaseDirectory, "tokens");
			});

			services.AddScopedSetting<AkeneoTenantSettings>(configuration, "Akeneo");

			services
				.AddMultiTenancy<AkeneoTenant>()
				.WithResolutionStrategy<AkeneoTenantResolutionStrategy>()
				.WithStore<ConfigurationTenantStore<AkeneoTenant, AkeneoTenantSettings>>();

			services.AddScoped(sp => sp.GetRequiredService<ITenantAccessor<AkeneoTenant>>().Tenant!);

			services.AddScoped<TenantRequiredResourceFilter<AkeneoTenant>>();

			return services;
		}
	}

	public static class IApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseAkeneo(this IApplicationBuilder builder)
			=> builder.UseMiddleware<TenantMiddleware<AkeneoTenant>>();
	}
}
