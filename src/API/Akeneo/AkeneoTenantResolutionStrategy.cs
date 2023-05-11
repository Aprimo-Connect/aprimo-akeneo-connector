using API.Multitenancy;

namespace API.Akeneo
{
	public class AkeneoTenantResolutionStrategy : ITenantResolutionStrategy<AkeneoTenant>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AkeneoTenantResolutionStrategy(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public Task<string> GetTenantIdentifierAsync()
		{
			if (TryGetFromCookie(out var tenantIdentifier))
			{
				return Task.FromResult(tenantIdentifier);
			}

			if (TryGetFromHeader(out tenantIdentifier))
			{
				return Task.FromResult(tenantIdentifier);
			}

			if (TryGetFromQueryString(out tenantIdentifier))
			{
				return Task.FromResult(tenantIdentifier);
			}

			return Task.FromResult(string.Empty);
		}

		public bool TryGetFromCookie(out string tenantId)
		{
			tenantId = _httpContextAccessor.HttpContext?.Request.Cookies[AkeneoConstants.AkeneoTenantCookieName] ?? "";
			if (string.IsNullOrEmpty(tenantId))
			{
				return false;
			}
			return true;
		}

		public bool TryGetFromHeader(out string tenantId)
		{
			tenantId = _httpContextAccessor.HttpContext?.Request.Headers[AkeneoConstants.AkeneoTenantHostHeader].ToString() ?? "";
			if (string.IsNullOrEmpty(tenantId))
			{
				return false;
			}

			return true;
		}

		public bool TryGetFromQueryString(out string tenantId)
		{
			var pimUrl = _httpContextAccessor.HttpContext?.Request.Query["pim_url"].ToString() ?? "";
			if (string.IsNullOrEmpty(pimUrl))
			{
				tenantId = "";
				return false;
			}

			var url = new Uri(pimUrl);
			tenantId = url.Host;
			return true;
		}
	}
}
