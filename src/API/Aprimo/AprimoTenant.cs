using API.Configuration;

namespace API.Aprimo
{
	public class AprimoTenant : TenantWithSettings<AprimoTenantSettings>
	{
		public AprimoTenant(string id, AprimoTenantSettings settings) : base(id, settings)
		{
		}

		public Uri OAuthBaseUri => new Uri($"https://{Id}.aprimo.com");

		public Uri DAMBaseUri => new Uri($"https://{Id}.dam.aprimo.com");
	}
}
