namespace API.Configuration
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddTransient<IStartupFilter, SettingValidationStartupFilter>();

			return services;
		}

		public static IServiceCollection AddScopedSetting<T>(this IServiceCollection services, IConfiguration configuration, string baseSectionName) where T : ScopedSetting, new()
		{
			services.AddSingleton<ScopedSettings<T>>(sp =>
			{
				if (!TryGetConfigurationSectionWithNamingFallback(configuration, baseSectionName, out var section) || section == null)
				{
					throw new InvalidProgramException($"Missing configuration section: {baseSectionName}");
				}

				var settings = new List<T>();
				var children = (section.GetChildren() ?? Array.Empty<IConfigurationSection>()).Where(child => !child.Key.Equals("_default", StringComparison.InvariantCultureIgnoreCase));
				foreach (var child in children)
				{
					var setting = new T();
					child.Bind(setting);
					setting.Scope = child.Key;
					settings.Add(setting);
				}

				return new ScopedSettings<T>(settings);
			});
			services.AddSingleton<IValidatable>(sp => sp.GetRequiredService<ScopedSettings<T>>());

			return services;
		}

		private static bool TryGetConfigurationSectionWithNamingFallback(IConfiguration configuration, string sectionName, out IConfigurationSection? foundSection)
		{
			var section = configuration.GetSection(sectionName);
			if (section.Exists())
			{
				foundSection = section;
				return true;
			}

			var sectionWithNamingFallback = configuration.GetSection(sectionName.Replace("Settings", ""));
			if (sectionWithNamingFallback.Exists())
			{
				foundSection = sectionWithNamingFallback;
				return true;
			}

			foundSection = null;
			return false;
		}
	}
}
