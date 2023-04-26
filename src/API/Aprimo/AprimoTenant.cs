using API.Configuration;

namespace API.Aprimo
{
	public class AprimoTenant : TenantWithSettings<AprimoTenantSettings>
	{
		public AprimoTenant(string id, AprimoTenantSettings settings) : base(id, settings)
		{
		}

		public Uri GetOAuthBaseUri()
		{
			return new Uri($"https://{Id}.aprimo.com");
		}

		public Uri GetAprimoDAMBaseUri()
		{
			return new Uri($"https://{Id}.dam.aprimo.com");
		}
	}
}
