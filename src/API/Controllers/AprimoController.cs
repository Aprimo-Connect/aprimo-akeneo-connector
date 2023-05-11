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
		private readonly IAkeneoTokenService _akeneoTokenService;
		private readonly IAprimoTokenService _aprimoTokenService;
		private readonly IAprimoToAkeneoIntegrationService _integrationService;
		private readonly IAprimoService _aprimoService;

		public AprimoController(
			ILogger<AprimoController> logger,
			IWebHostEnvironment env,
			IAkeneoTokenService akeneoService,
			IAprimoTokenService aprimoTokenService,
			IAprimoToAkeneoIntegrationService integrationService,
			IAprimoService aprimoService)
		{
			_logger = logger;
			_env = env;
			_akeneoTokenService = akeneoService;
			_aprimoTokenService = aprimoTokenService;
			_integrationService = integrationService;
			_aprimoService = aprimoService;
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
			if (!(await _akeneoTokenService.IsConfigured()))
			{
				return StatusCode(StatusCodes.Status417ExpectationFailed, "Akeneo is not yet connected.");
			}

			if (string.IsNullOrEmpty(syncRequest?.RecordId))
			{
				return BadRequest("recordId is required");
			}

			var (loadCurrentUserSuccess, user) = await _aprimoService.GetCurrentUser();
			if (!loadCurrentUserSuccess || !user.HasValue)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Failed to load integration user.");
			}

			if (user.Value.AdamUserId.Equals(syncRequest.UserId))
			{
				_logger.LogInformation("Skipping sync for user {userId} because it is the integration user.", syncRequest.UserId);
				return Ok($"Skipping sync for user {syncRequest.UserId} because it is the integration user.");
			}

			var (syncSuccess, syncResult) = await _integrationService.SendAprimoDAMRecordToAkeneo(syncRequest);

			if (!syncSuccess || syncResult == null)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, "Failed to sync record to Akeneo.");
			}

			return Ok($"Created {syncResult}");
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
