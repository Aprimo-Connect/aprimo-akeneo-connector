namespace API.Integration
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddIntegration(this IServiceCollection services)
		{
			services.AddTransient<IAprimoToAkeneoIntegrationService, AprimoToAkeneoIntegrationService>();

			return services;
		}
	}
}
