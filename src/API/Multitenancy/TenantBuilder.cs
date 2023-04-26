using Microsoft.Extensions.DependencyInjection.Extensions;

namespace API.Multitenancy
{
	public class TenantBuilder<T> where T : class, ITenant
	{
		private readonly IServiceCollection _services;

		public TenantBuilder(IServiceCollection services)
		{
			_services = services;
			_services.AddTransient<TenantAccessService<T>>();
			_services.AddTransient<ITenantAccessor<T>, TenantAccessor<T>>();
		}

		/// <summary>
		/// Register the tenant resolver implementation
		/// </summary>
		/// <typeparam name="V"></typeparam>
		/// <param name="lifetime"></param>
		/// <returns></returns>
		public TenantBuilder<T> WithResolutionStrategy<V>(ServiceLifetime lifetime = ServiceLifetime.Transient) where V : class, ITenantResolutionStrategy<T>
		{
			_services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			_services.Add(ServiceDescriptor.Describe(typeof(ITenantResolutionStrategy<T>), typeof(V), lifetime));
			return this;
		}

		/// <summary>
		/// Register the tenant store implementation
		/// </summary>
		/// <typeparam name="V"></typeparam>
		/// <param name="lifetime"></param>
		/// <returns></returns>
		public TenantBuilder<T> WithStore<V>(ServiceLifetime lifetime = ServiceLifetime.Transient) where V : class, ITenantStore<T>
		{
			_services.Add(ServiceDescriptor.Describe(typeof(ITenantStore<T>), typeof(V), lifetime));
			return this;
		}
	}
}
