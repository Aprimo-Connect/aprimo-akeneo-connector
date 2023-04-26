using API.Multitenancy;

namespace API.Configuration
{
	public class ConfigurationTenantStore<TTenant, TSetting> : ITenantStore<TTenant> where TTenant : TenantWithSettings<TSetting> where TSetting : ScopedSetting
	{
		private readonly ScopedSettings<TSetting> _settings;

		public ConfigurationTenantStore(ScopedSettings<TSetting> settings)
		{
			_settings = settings;
		}

		public Task<TTenant?> GetTenantAsync(string identifier)
		{
			if (string.IsNullOrEmpty(identifier))
			{
				return Task.FromResult<TTenant?>(null);
			}

			var tenantSettings = _settings.Get(identifier);

			if (tenantSettings == null)
			{
				return Task.FromResult<TTenant?>(null);
			}

			var tenant = Activator.CreateInstance(typeof(TTenant), identifier, tenantSettings) as TTenant;

			return Task.FromResult(tenant);
		}
	}
}
