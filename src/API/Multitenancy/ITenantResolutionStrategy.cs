namespace API.Multitenancy
{
	public interface ITenantResolutionStrategy<T> where T : ITenant
	{
		Task<string> GetTenantIdentifierAsync();
	}
}
