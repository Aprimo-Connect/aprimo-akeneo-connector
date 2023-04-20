using API.Akeneo;
using API.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AkeneoController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly AkeneoSettings _settings;
		private readonly IDataProtector _dataProtector;
		private readonly IAkeneoService _akeneoService;

		public AkeneoController(ILogger<AkeneoController> logger, AkeneoSettings settings, IDataProtectionProvider dataProtectionProvider, IAkeneoService akeneoService)
		{
			_logger = logger;
			_settings = settings;
			_dataProtector = dataProtectionProvider.CreateProtector("AkeneoSettings");
			_akeneoService = akeneoService;
		}

		/// <summary>
		/// Activation endpoint for AkeneoSettings PIM (<a href="https://api.akeneo.com/apps/authentication-and-authorization.html">AkeneoSettings OAuth documentation</a>).
		/// </summary>
		/// <param name="pim_url">The URL to AkeneoSettings (e.g. https://xxx.cloud.akeneo.com/)</param>
		/// <returns></returns>
		[HttpGet("activate", Name = "Activate")]
		public IActionResult Activate([Required] Uri pim_url)
		{
			if (!_settings.IsAllowedHost(pim_url.Host))
			{
				_logger.LogWarning("Host {host} is not allowed. Make sure to add the host to the configuration {configuration}.", pim_url.Host, $"{nameof(AkeneoSettings)}.{nameof(AkeneoSettings.AllowedHosts)}");

				return new StatusCodeResult(StatusCodes.Status403Forbidden);
			}

			var redirectUriBuilder = new UriBuilder(pim_url);
			redirectUriBuilder.Path = "/connect/apps/v1/authorize";

			var queryString = new Dictionary<string, string?>
			{
				{ "response_type", "code" },
				{ "client_id", _settings.ClientId },
				{ "scope", _settings.Scopes },
				{ "state", _dataProtector.Protect(pim_url.ToString()) },
			};

			var redirectUri = QueryHelpers.AddQueryString(redirectUriBuilder.Uri.ToString(), queryString);
			return Redirect(redirectUri);
		}

		/// <summary>
		/// Callback endpoint for AkeneoSettings PIM (<a href="https://api.akeneo.com/apps/authentication-and-authorization.html">AkeneoSettings OAuth documentation</a>).
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

			var akeneoOAuthResult = await _akeneoService.TryGetGetOAuthToken(pimUrl, code);
			if (!akeneoOAuthResult.Success)
			{
				return Problem();
			}

			return Ok(akeneoOAuthResult.TokenResponse);
		}
	}
}
