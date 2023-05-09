namespace API.Multitenancy
{
	public interface ITenantStore<T> where T : ITenant
	{
		Task<T?> GetTenantAsync(string identifier);
	}
}
