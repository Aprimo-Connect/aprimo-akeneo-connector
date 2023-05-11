using API.Configuration;

namespace API.Aprimo
{
	public class AprimoTenant : TenantWithSettings<AprimoTenantSettings>
	{
		public AprimoTenant(string id, AprimoTenantSettings settings) : base(id, settings)
		{
		}

		public Uri DAMAPIBaseUri => new Uri($"https://{Id}.dam.aprimo.com");

		public Uri APIBaseUri => new Uri($"https://{Id}.aprimo.com");

		public Uri OAuthBaseUri => APIBaseUri;
	}
}
