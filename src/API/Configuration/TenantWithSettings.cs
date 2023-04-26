using API.Multitenancy;

namespace API.Configuration
{
	public class TenantWithSettings<T> : ITenant where T : class
	{
		public string Id { get; private set; }

		public readonly T Settings;

		public TenantWithSettings(string id, T settings)
		{
			Id = id;
			Settings = settings;
		}
	}
}
