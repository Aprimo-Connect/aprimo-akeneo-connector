using API.Akeneo;
using API.Aprimo;
using API.Aprimo.Models;
using API.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
		private readonly IAprimoService _aprimoService;
		private readonly IAkeneoService _akeneoService;
		private readonly IAprimoTokenService _aprimoTokenService;

		public AprimoController(ILogger<AprimoController> logger, IWebHostEnvironment env, AprimoTenant aprimoTenant, AkeneoTenant akeneoTenant, IAprimoService aprimoService, IAkeneoService akeneoService, IAprimoTokenService aprimoTokenService)
		{
			_logger = logger;
			_env = env;
			_aprimoTenant = aprimoTenant;
			_akeneoTenant = akeneoTenant;
			_aprimoService = aprimoService;
			_akeneoService = akeneoService;
			_aprimoTokenService = aprimoTokenService;
		}

		/// <summary>
		/// Endpoint for the Aprimo DAM Rule when an Asset is changed or created.
		/// </summary>
		/// <param name="aprimoRuleBody">The data from the Aprimo rule</param>
		/// <returns></returns>
		[HttpPost("execute", Name = "Execute")]
		[Authorize(AuthenticationSchemes = "AprimoRuleAuth")]
		[ServiceFilter(typeof(AprimoHMACResourceFilter))]
		public async Task<IActionResult> Execute([FromBody] AprimoRuleBody aprimoRuleBody)
		{
			if (!(await _akeneoService.IsConfigured()))
			{
				return StatusCode(StatusCodes.Status417ExpectationFailed, "Akeneo is not yet connected.");
			}

			if (string.IsNullOrEmpty(aprimoRuleBody?.RecordId))
			{
				return BadRequest("recordId is required");
			}

			var (didLoadRecord, record) = await _aprimoService.GetRecord(aprimoRuleBody.RecordId);
			if (!didLoadRecord)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Failed to load Aprimo DAM record.");
			}

			return Ok(record);
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

			var tokenResult = await _aprimoTokenService.TryGetTokenAsync();
			if (!tokenResult.Success)
			{
				return BadRequest();
			}

			return Ok(tokenResult.Token);
		}
	}
}
