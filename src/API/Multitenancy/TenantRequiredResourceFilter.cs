using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Multitenancy
{
	public class TenantRequiredResourceFilter<T> : IAsyncResourceFilter where T : class, ITenant
	{
		private readonly ILogger _logger;

		public TenantRequiredResourceFilter(ILogger<TenantRequiredResourceFilter<T>> logger)
		{
			_logger = logger;
		}

		public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
		{
			var tenant = context.HttpContext.GetTenant<T>();
			if (tenant == null)
			{
				_logger.LogError("Tenant not found.");
				context.Result = new ObjectResult($"{typeof(T).Name} is unknown.") { StatusCode = StatusCodes.Status401Unauthorized };
				return;
			}

			if (next != null)
			{
				await next();
			}
		}
	}
}
