namespace API.Multitenancy
{
	internal class TenantAccessService<T> where T : ITenant
	{
		private readonly ITenantResolutionStrategy<T> _tenantResolutionStrategy;
		private readonly ITenantStore<T> _tenantStore;

		public TenantAccessService(ITenantResolutionStrategy<T> tenantResolutionStrategy, ITenantStore<T> tenantStore)
		{
			_tenantResolutionStrategy = tenantResolutionStrategy;
			_tenantStore = tenantStore;
		}

		/// <summary>
		/// Get the current tenant
		/// </summary>
		/// <returns></returns>
		public async Task<T?> GetTenantAsync()
		{
			var tenantIdentifier = await _tenantResolutionStrategy.GetTenantIdentifierAsync();
			return await _tenantStore.GetTenantAsync(tenantIdentifier);
		}
	}
}
