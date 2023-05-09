using API._Common;
using API.Configuration;
using API.Multitenancy;
using Polly;

namespace API.Akeneo
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddAkeneo(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
		{
			services.AddDefaultHttpClient<IAkeneoTokenService, AkeneoTokenService>(env);
			services
				.AddScoped<AkeneoTokenAuthHeaderHandler>()
				.AddDefaultHttpClient<IAkeneoService, AkeneoService>(env)
				.AddHttpMessageHandler<AkeneoTokenAuthHeaderHandler>()
				.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, (attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

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
