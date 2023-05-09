using API.Multitenancy;

namespace API.Aprimo
{
	public class AprimoTenantResolutionStrategy : ITenantResolutionStrategy<AprimoTenant>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AprimoTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public Task<string> GetTenantIdentifierAsync()
		{
			if (TryGetFromBasicAuth(out var tenantIdentifier))
			{
				return Task.FromResult(tenantIdentifier);
			}

			if (TryGetFromHeader(out tenantIdentifier))
			{
				return Task.FromResult(tenantIdentifier);
			}

			return Task.FromResult(string.Empty);
		}

		public bool TryGetFromBasicAuth(out string tenantId)
		{
			tenantId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "";
			if (string.IsNullOrEmpty(tenantId))
			{
				return false;
			}
			return true;
		}

		public bool TryGetFromHeader(out string tenantId)
		{
			tenantId = _httpContextAccessor.HttpContext?.Request.Headers["x-aprimo-tenant"] ?? "";
			if (string.IsNullOrEmpty(tenantId))
			{
				return false;
			}

			return true;
		}
	}
}
