using API.Multitenancy;
using System.Security.Claims;

namespace API.Aprimo
{
	public class AprimoUserRepository : IAprimoUserRepository
	{
		private readonly IConfiguration _config;
		private readonly ILogger _logger;
		private readonly ITenantStore<AprimoTenant> _tenantStore;

		public AprimoUserRepository(ILogger<AprimoUserRepository> logger, IConfiguration config, ITenantStore<AprimoTenant> tenantStore)
		{
			_logger = logger;
			_config = config;
			_tenantStore = tenantStore;
		}

		public async Task<(bool Success, IEnumerable<Claim> Claims)> Authenticate(string username, string password)
		{
			var tenant = await _tenantStore.GetTenantAsync(username);
			if (tenant == null || string.IsNullOrEmpty(tenant.Settings.BasicAuthPassword))
			{
				(bool, IEnumerable<Claim>) failedResult = (false, new List<Claim>());
				return failedResult;
			}

			(bool, IEnumerable<Claim>) successResult = (tenant.Settings.BasicAuthPassword.Equals(password), new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
			});

			return successResult;
		}
	}
}
