namespace API.Multitenancy
{
	public class TenantAccessor<T> : ITenantAccessor<T> where T : class, ITenant
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public TenantAccessor(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		public T? Tenant => _httpContextAccessor.HttpContext?.GetTenant<T>() ?? null;
	}
}
