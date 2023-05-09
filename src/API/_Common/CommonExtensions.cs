using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API._Common
{
	public static class IServiceCollectionExtensions
	{
		public static IHttpClientBuilder AddDefaultHttpClient<TClient, TImplementation>(this IServiceCollection services, IHostEnvironment env) where TClient : class where TImplementation : class, TClient
		{
			return services.AddDefaultHttpClient<TClient, TImplementation>(env, (client) =>
			{
				client.Timeout = TimeSpan.FromSeconds(10);
			});
		}

		public static IHttpClientBuilder AddDefaultHttpClient<TClient, TImplementation>(this IServiceCollection services, IHostEnvironment env, Action<HttpClient> configureHttpClient) where TClient : class where TImplementation : class, TClient
		{
			if (env.IsDevelopment())
			{
				services.AddHttpLogger();
			}

			var httpClientBuilder = services
				.AddHttpClient<TClient, TImplementation>()
				.ConfigureHttpClient(client =>
				{
					client.AddDefaultUserAgent();
					configureHttpClient(client);
				});

			if (env.IsDevelopment())
			{
				httpClientBuilder.AddHttpLogger();
			}

			return httpClientBuilder;
		}

		public static IServiceCollection AddHttpLogger(this IServiceCollection services)
		{
			services.TryAddScoped<HttpClientLoggingHandler>();

			return services;
		}
	}

	public static class IHttpClientBuilderExtensions
	{
		public static IHttpClientBuilder AddHttpLogger(this IHttpClientBuilder builder)
		{
			return builder.AddHttpMessageHandler<HttpClientLoggingHandler>();
		}
	}

	public static class HttpClientExtensions
	{
		public static void AddDefaultUserAgent(this HttpClient client)
		{
			client.DefaultRequestHeaders.Add("User-Agent", $"Aprimo.Akeneo.Connector/{Environment.MachineName}");
		}
	}
}
