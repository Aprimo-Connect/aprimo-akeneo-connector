using Microsoft.Extensions.Options;

namespace API.Configuration
{
	public class AkeneoSettings : IValidatable
	{
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string? AllowedHosts { get; set; }
		public string? Scopes { get; set; }

		public void Validate()
		{
			if (string.IsNullOrEmpty(ClientId))
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AkeneoSettings)}:{nameof(ClientId)}");
			}
			if (string.IsNullOrEmpty(ClientSecret))
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AkeneoSettings)}:{nameof(ClientSecret)}");
			}
			if (string.IsNullOrEmpty(Scopes))
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AkeneoSettings)}:{nameof(Scopes)}");
			}
			if (string.IsNullOrEmpty(AllowedHosts))
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AkeneoSettings)}:{nameof(AllowedHosts)}");
			}
		}

		public bool IsAllowedHost(string host)
		{
			if (string.IsNullOrEmpty(AllowedHosts))
			{
				return false;
			}

			return AllowedHosts
				.Split(',')
				.Select(allowedHost => allowedHost.Trim())
				.Any(allowedHost => allowedHost.Equals("*") || host.Equals(allowedHost, StringComparison.InvariantCultureIgnoreCase));
		}
	}

	public class AprimoSettings : IValidatable
	{
		public string? ClientId { get; set; }

		public string? ClientSecret { get; set; }

		public Dictionary<string, string>? Users { get; set; }

		public void Validate()
		{
			if (string.IsNullOrEmpty(ClientId))
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AprimoSettings)}:{nameof(ClientId)}");
			}

			if (string.IsNullOrEmpty(ClientSecret))
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AprimoSettings)}:{nameof(ClientSecret)}");
			}

			if (Users == null || Users.Count <= 0)
			{
				throw new InvalidProgramException($"Missing required configuration value: {nameof(AprimoSettings)}:{nameof(Users)}");
			}
		}
	}

	public static class IServiceCollection_Extensions
	{
		public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

			services.AddValidatingSettings<AkeneoSettings>(configuration, nameof(AkeneoSettings));
			services.AddValidatingSettings<AprimoSettings>(configuration, nameof(AprimoSettings));

			return services;
		}

		public static IServiceCollection AddValidatingSettings<T>(this IServiceCollection services, IConfiguration configuration, string sectionName) where T : class, IValidatable
		{
			services.ConfigureExplicitly<T>(configuration, sectionName);
			services.AddSingleton<IValidatable>(resolver => resolver.GetRequiredService<IOptions<T>>().Value);
			return services;
		}

		public static IServiceCollection ConfigureExplicitly<T>(this IServiceCollection services, IConfiguration configuration, string sectionName) where T : class
		{
			var configSection = GetConfigurationSectionWithNamingFallback(configuration, sectionName);
			if (configSection == null)
			{
				throw new InvalidProgramException($"Missing configuration section: {sectionName}");
			}

			services.Configure<T>(configSection);
			services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<T>>().Value);
			return services;
		}

		private static IConfigurationSection? GetConfigurationSectionWithNamingFallback(IConfiguration configuration, string sectionName)
		{
			var section = configuration.GetSection(sectionName);
			if (section.Exists())
			{
				return section;
			}

			var sectionWithNamingFallback = configuration.GetSection(sectionName.Replace("Settings", ""));
			if (sectionWithNamingFallback.Exists())
			{
				return sectionWithNamingFallback;
			}

			return null;
		}
	}
}
