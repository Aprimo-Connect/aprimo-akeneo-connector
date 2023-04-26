namespace API.Multitenancy
{
	internal class TenantMiddleware<T> where T : class, ITenant
	{
		private readonly RequestDelegate next;

		public TenantMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			await context.SetTenant(async () =>
			{
				var tenantService = context.RequestServices.GetRequiredService<TenantAccessService<T>>();
				return await tenantService.GetTenantAsync();
			});

			if (next != null)
			{
				await next(context);
			}
		}
	}
}
