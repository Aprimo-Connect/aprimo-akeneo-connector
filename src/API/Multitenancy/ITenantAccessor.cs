namespace API.Multitenancy
{
	public interface ITenantAccessor<T> where T : ITenant
	{
		T? Tenant { get; }
	}
}
