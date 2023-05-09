using API._Common;
using API.Configuration;
using API.Multitenancy;
using Polly;

namespace API.Aprimo
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddAprimo(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
		{
			services.AddDefaultHttpClient<IAprimoTokenService, AprimoTokenService>(env);

			var unauthorizedPolicy = Policy.HandleResult<HttpResponseMessage>(message => message.StatusCode == System.Net.HttpStatusCode.Unauthorized).RetryAsync(1);
			services
				.AddScoped<AprimoTokenAuthHeaderHandler>()
				.AddDefaultHttpClient<IAprimoService, AprimoService>(env)
				.ConfigureHttpClient((client) =>
				{
					client.DefaultRequestHeaders.Add("API-VERSION", "1");
					client.DefaultRequestHeaders.Add("Accept", "application/json");
					client.Timeout = TimeSpan.FromMinutes(1);
				})
				.AddPolicyHandler(unauthorizedPolicy)
				.AddHttpMessageHandler<AprimoTokenAuthHeaderHandler>()
				.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(3, (attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

			services.AddScoped<IAprimoUserRepository, AprimoUserRepository>();
			services.AddScoped<AprimoHMACResourceFilter>();
			services.AddScopedSetting<AprimoTenantSettings>(configuration, "Aprimo");

			services
				.AddMultiTenancy<AprimoTenant>()
				.WithResolutionStrategy<AprimoTenantResolutionStrategy>()
				.WithStore<ConfigurationTenantStore<AprimoTenant, AprimoTenantSettings>>();

			services.AddScoped(sp => sp.GetRequiredService<ITenantAccessor<AprimoTenant>>().Tenant!);

			services.AddScoped<TenantRequiredResourceFilter<AprimoTenant>>();

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
