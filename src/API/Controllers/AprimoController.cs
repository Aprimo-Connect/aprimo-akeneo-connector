using API.Akeneo;
using API.Aprimo;
using API.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AprimoController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IHostEnvironment _env;
		private readonly AprimoSettings _aprimoSettings;
		private readonly AkeneoSettings _akeneoSettings;
		private readonly IAprimoTokenService _aprimoTokenService;
		private readonly IAkeneoService _akeneoService;

		public AprimoController(ILogger<AprimoController> logger, IWebHostEnvironment env, AprimoSettings aprimoSettings, AkeneoSettings akeneoSettings, IAprimoTokenService aprimoTokenService, IAkeneoService akeneoService)
		{
			_logger = logger;
			_env = env;
			_aprimoSettings = aprimoSettings;
			_akeneoSettings = akeneoSettings;
			_aprimoTokenService = aprimoTokenService;
			_akeneoService = akeneoService;
		}

		/// <summary>
		/// Endpoint for the Aprimo DAM Rule when an Asset is changed or created.
		/// </summary>
		/// <param name="pim_url">The URL to Akeneo (e.g. https://xxx.cloud.akeneo.com/)</param>
		/// <param name="aprimoRuleBody">The data from the Aprimo rule</param>
		/// <returns></returns>
		[HttpPost("execute", Name = "Execute")]
		[Authorize(AuthenticationSchemes = "AprimoRuleAuth")]
		[ServiceFilter(typeof(AprimoHMACResourceFilter))]
		public async Task<IActionResult> Execute([Required] Uri pim_url, [FromBody] AprimoRuleBody aprimoRuleBody)
		{
			if (!_akeneoSettings.IsAllowedHost(pim_url.Host))
			{
				_logger.LogWarning("Host {host} is not allowed. Make sure to add the host to the configuration {configuration}.", pim_url.Host, $"{nameof(AkeneoSettings)}.{nameof(AkeneoSettings.AllowedHosts)}");

				return StatusCode(StatusCodes.Status400BadRequest, "Invalid pim_url. Host is not allowed.");
			}

			var akeneoToken = await _akeneoService.GetCurrentTokenForHost(pim_url.Host);
			if (string.IsNullOrEmpty(akeneoToken))
			{
				return StatusCode(StatusCodes.Status417ExpectationFailed, "No current access token available to Akeneo.");
			}

			return Ok();
		}

		[HttpGet("auth")]
		[Authorize(AuthenticationSchemes = "AprimoRuleAuth")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> Authenticate()
		{
			if (!_env.IsDevelopment())
			{
				return NotFound();
			}

			if (!User.Identity?.IsAuthenticated ?? true)
			{
				return BadRequest();
			}

			var userName = User.Identity?.Name;
			if (string.IsNullOrEmpty(userName))
			{
				return BadRequest();
			}

			var tokenResult = await _aprimoTokenService.GetTokenAsync(new Uri($"https://{userName}"), _aprimoSettings.ClientId!, _aprimoSettings.ClientSecret!);
			if (!tokenResult.Success)
			{
				return BadRequest();
			}

			return Ok(tokenResult.Token);
		}
	}
}
