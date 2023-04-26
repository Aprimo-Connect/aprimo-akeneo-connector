using API.Configuration;
using API.Multitenancy;

namespace API.Aprimo
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddAprimo(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddHttpClient<IAprimoTokenService, AprimoTokenService>();
			services.AddScoped<IAprimoUserRepository, AprimoUserRepository>();
			services.AddScoped<AprimoHMACResourceFilter>();
			services.AddScopedSetting<AprimoTenantSettings>(configuration, "Aprimo");

			services
				.AddMultiTenancy<AprimoTenant>()
				.WithResolutionStrategy<AprimoTenantResolutionStrategy>()
				.WithStore<ConfigurationTenantStore<AprimoTenant, AprimoTenantSettings>>();

			services.AddScoped(sp => sp.GetRequiredService<ITenantAccessor<AprimoTenant>>().Tenant!);

			services.AddAuthentication().AddScheme<AprimoRuleAuthenticationHandlerOptions, AprimoRuleAuthenticationHandler>("AprimoRuleAuth", null);

			return services;
		}
	}

	public static class IApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseAprimo(this IApplicationBuilder builder)
			=> builder.UseMiddleware<TenantMiddleware<AprimoTenant>>();
	}
}
