namespace API.Multitenancy
{
	public static class HttpContextExtensions
	{
		private static string GetTenantKey<T>()
		{
			return $"Tenant:{typeof(T).Name}";
		}

		public static async Task SetTenant<T>(this HttpContext context, Func<Task<T?>> tenantLookup) where T : class, ITenant
		{
			var key = GetTenantKey<T>();
			if (!context.Items.ContainsKey(key))
			{
				var tenant = await tenantLookup();
				context.Items.Add(key, tenant);
			}
		}

		public static T? GetTenant<T>(this HttpContext context) where T : class, ITenant
		{
			var key = GetTenantKey<T>();
			if (!context.Items.ContainsKey(key))
			{
				return default;
			}

			return context.Items[key] as T;
		}
	}

	public static class ServiceCollectionExtensions
	{
		public static TenantBuilder<T> AddMultiTenancy<T>(this IServiceCollection services) where T : class, ITenant
			=> new TenantBuilder<T>(services);
	}
}
