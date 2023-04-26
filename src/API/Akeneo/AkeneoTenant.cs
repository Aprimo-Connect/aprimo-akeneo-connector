using API.Configuration;

namespace API.Akeneo
{
	public class AkeneoTenant : TenantWithSettings<AkeneoTenantSettings>
	{
		public AkeneoTenant(string id, AkeneoTenantSettings settings) : base(id, settings)
		{
		}
	}
}
