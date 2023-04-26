using API.Akeneo;
using API.Aprimo;
using API.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	[ServiceFilter(typeof(TenantRequiredResourceFilter<AkeneoTenant>))]
	[ServiceFilter(typeof(TenantRequiredResourceFilter<AprimoTenant>))]
	public class AprimoController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IHostEnvironment _env;
		private readonly AprimoTenant _aprimoTenant;
		private readonly AkeneoTenant _akeneoTenant;
		private readonly IAprimoTokenService _aprimoTokenService;
		private readonly IAkeneoService _akeneoService;

		public AprimoController(ILogger<AprimoController> logger, IWebHostEnvironment env, AprimoTenant aprimoTenant, AkeneoTenant akeneoTenant, IAprimoTokenService aprimoTokenService, IAkeneoService akeneoService)
		{
			_logger = logger;
			_env = env;
			_aprimoTenant = aprimoTenant;
			_akeneoTenant = akeneoTenant;
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
			var akeneoToken = await _akeneoService.GetCurrentTokenForHost(_akeneoTenant.Id);
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

			var tokenResult = await _aprimoTokenService.GetTokenAsync(_aprimoTenant.GetOAuthBaseUri(), _aprimoTenant.Settings.ClientId!, _aprimoTenant.Settings.ClientSecret!);
			if (!tokenResult.Success)
			{
				return BadRequest();
			}

			return Ok(tokenResult.Token);
		}
	}
}
