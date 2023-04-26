using API.Akeneo;
using API.Multitenancy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[ServiceFilter(typeof(TenantRequiredResourceFilter<AkeneoTenant>))]
	public class AkeneoController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IDataProtector _dataProtector;
		private readonly IAkeneoService _akeneoService;
		private readonly ITokenStorage _tokenStorage;
		private readonly IHostEnvironment _env;
		private readonly AkeneoTenant _tenant;

		public AkeneoController(ILogger<AkeneoController> logger, IDataProtectionProvider dataProtectionProvider, IAkeneoService akeneoService, ITokenStorage tokenStorage, IWebHostEnvironment env, AkeneoTenant tenant)
		{
			_logger = logger;
			_dataProtector = dataProtectionProvider.CreateProtector("AkeneoSettings");
			_akeneoService = akeneoService;
			_tokenStorage = tokenStorage;
			_env = env;
			_tenant = tenant;
		}

		/// <summary>
		/// Activation endpoint for Akeneo PIM (<a href="https://api.akeneo.com/apps/authentication-and-authorization.html">Akeneo OAuth documentation</a>).
		/// </summary>
		/// <param name="pim_url">The URL to Akeneo (e.g. https://xxx.cloud.akeneo.com/)</param>
		/// <returns></returns>
		[HttpGet("activate", Name = "Activate")]
		public IActionResult Activate([Required] Uri pim_url)
		{
			if (!pim_url.Host.Equals(_tenant.Id))
			{
				_logger.LogError("The host {host} does not match the tenant id {tenantId}", pim_url.Host, _tenant.Id);
				return new ObjectResult($"The host '{pim_url.Host}' is not allowed for this tenant.") { StatusCode = StatusCodes.Status403Forbidden };
			}

			var redirectUriBuilder = new UriBuilder(pim_url);
			redirectUriBuilder.Path = "/connect/apps/v1/authorize";

			var queryString = new Dictionary<string, string?>
			{
				{ "response_type", "code" },
				{ "client_id", _tenant.Settings.ClientId },
				{ "scope", _tenant.Settings.Scopes },
				{ "state", _dataProtector.Protect(pim_url.ToString()) },
			};

			var redirectUri = QueryHelpers.AddQueryString(redirectUriBuilder.Uri.ToString(), queryString);
			Response.Cookies.Append(AkeneoConstants.AkeneoTenantCookieName, _tenant.Id, new CookieOptions
			{
				HttpOnly = true,
				MaxAge = TimeSpan.FromMinutes(5)
			});
			return Redirect(redirectUri);
		}

		/// <summary>
		/// Callback endpoint for Akeneo PIM (<a href="https://api.akeneo.com/apps/authentication-and-authorization.html">Akeneo OAuth documentation</a>).
		/// </summary>
		/// <returns></returns>
		[HttpGet("callback", Name = "Callback")]
		public async Task<IActionResult> Callback([Required] string state, [Required] string code)
		{
			Uri? pimUrl;
			try
			{
				pimUrl = new Uri(_dataProtector.Unprotect(state));
			}
			catch (Exception e) when (e is CryptographicException || e is NullReferenceException || e is UriFormatException)
			{
				_logger.LogError("Invalid state {state}: {error}", state, e);
				return BadRequest();
			}

			if (!pimUrl.Host.Equals(_tenant.Id))
			{
				_logger.LogError("The host {host} does not match the tenant id {tenantId}", pimUrl.Host, _tenant.Id);
				return new ObjectResult($"The host '{pimUrl.Host}' is not allowed for this tenant.") { StatusCode = StatusCodes.Status403Forbidden };
			}

			var (success, tokenResponse) = await _akeneoService.TryGetGetOAuthToken(pimUrl, code);
			if (!success)
			{
				return Problem();
			}

			Response.Cookies.Delete(AkeneoConstants.AkeneoTenantCookieName);
			return Ok(tokenResponse);
		}

		[HttpGet("token")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> Token([Required] Uri pim_url)
		{
			if (!_env.IsDevelopment())
			{
				return NotFound();
			}

			var token = await _tokenStorage.GetTokenAsync(pim_url.Host);
			if (string.IsNullOrEmpty(token))
			{
				return NotFound();
			}

			return Ok(token);
		}
	}
}
