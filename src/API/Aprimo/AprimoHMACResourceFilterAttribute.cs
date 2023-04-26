using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace API.Aprimo
{
	public class AprimoHMACResourceFilter : IAsyncResourceFilter
	{
		private readonly ILogger _logger;
		private readonly AprimoTenant _tenant;

		public AprimoHMACResourceFilter(ILogger<AprimoHMACResourceFilter> logger, AprimoTenant tenant)
		{
			_logger = logger;
			_tenant = tenant;
		}

		public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
		{
			var (success, didValidate) = await MaybeValidateHMACHeader(context.HttpContext.Request, _tenant.Settings.HMACSecret);
			if (!success)
			{
				_logger.LogError("HMAC validation failed (didValidate? {didValidate}).", didValidate);
				context.Result = new ObjectResult("HMAC validation failed.") { StatusCode = StatusCodes.Status403Forbidden };
				return;
			}

			await next();
		}

		private async Task<(bool Success, bool DidValidate)> MaybeValidateHMACHeader(HttpRequest request, string hmacSecret)
		{
			var hmacHeader = request.Headers["x-aprimo-hmac"].FirstOrDefault();
			if (string.IsNullOrEmpty(hmacHeader))
			{
				return (true, false);
			}

			if (string.IsNullOrEmpty(hmacSecret))
			{
				return (true, false);
			}

			// Enable buffering so we can rewind the stream.
			request.EnableBuffering();
			if (!await ValidateHMAC(hmacSecret, request.Body, hmacHeader))
			{
				return (false, true);
			}
			request.Body.Seek(0, SeekOrigin.Begin);

			return (true, true);
		}

		private async Task<bool> ValidateHMAC(string hmacSecret, Stream payload, string expectedValue)
		{
			var secret = Encoding.UTF8.GetBytes(hmacSecret);
			using (var hmac = new HMACSHA256(secret))
			{
				var hash = await hmac.ComputeHashAsync(payload);
				var hashString = Convert.ToBase64String(hash);
				return hashString == expectedValue;
			}
		}
	}
}
