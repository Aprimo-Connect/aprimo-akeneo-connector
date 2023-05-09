using API.Akeneo;
using API.Aprimo;
using API.Integration;
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
		private readonly IAprimoService _aprimoService;
		private readonly IAkeneoTokenService _akeneTokenService;
		private readonly IAprimoTokenService _aprimoTokenService;
		private readonly IAprimoToAkeneoIntegrationService _integrationService;

		public AprimoController(ILogger<AprimoController> logger, IWebHostEnvironment env, IAprimoService aprimoService, IAkeneoTokenService akeneoService, IAprimoTokenService aprimoTokenService, IAprimoToAkeneoIntegrationService integrationService)
		{
			_logger = logger;
			_env = env;
			_aprimoService = aprimoService;
			_akeneTokenService = akeneoService;
			_aprimoTokenService = aprimoTokenService;
			_integrationService = integrationService;
		}

		/// <summary>
		/// Endpoint for the Aprimo DAM Rule when an Asset is changed or created.
		/// </summary>
		/// <param name="syncRequest">The data from the Aprimo rule</param>
		/// <returns></returns>
		[HttpPost("execute", Name = "Execute")]
		[Authorize(AuthenticationSchemes = "AprimoRuleAuth")]
		[ServiceFilter(typeof(AprimoHMACResourceFilter))]
		public async Task<IActionResult> Execute([FromBody] SyncAprimoToAkeneoRequest syncRequest)
		{
			if (!(await _akeneTokenService.IsConfigured()))
			{
				return StatusCode(StatusCodes.Status417ExpectationFailed, "Akeneo is not yet connected.");
			}

			if (string.IsNullOrEmpty(syncRequest?.RecordId))
			{
				return BadRequest("recordId is required");
			}

			var (syncSuccess, syncResult) = await _integrationService.SendAprimoDAMRecordToAkeneo(syncRequest);

			if (!syncSuccess || syncResult == null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Failed to sync record to Akeneo.");
			}

			return Ok(syncResult);
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
