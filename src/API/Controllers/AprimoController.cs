using API.Aprimo;
using API.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AprimoController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly IHostEnvironment _env;
		private readonly AprimoSettings _settings;
		private readonly IAprimoTokenService _tokenService;

		public AprimoController(ILogger<AprimoController> logger, IWebHostEnvironment env, AprimoSettings settings, IAprimoTokenService aprimoTokenService)
		{
			_logger = logger;
			_env = env;
			_settings = settings;
			_tokenService = aprimoTokenService;
		}

		/// <summary>
		/// Endpoint for the Aprimo DAM Rule when an Asset is changed or created.
		/// </summary>
		/// <param name="pim_url">The URL to Akeneo (e.g. https://xxx.cloud.akeneo.com/)</param>
		/// <returns></returns>
		[HttpPost("execute", Name = "Execute")]
		[Authorize(AuthenticationSchemes = "AprimoRuleAuth")]
		public IActionResult Execute()
		{
			return Ok();
		}



		[HttpGet("auth")]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> Authenticate()
		{
			if (!_env.IsDevelopment())
			{
				return NotFound();
			}

			var tokenResult = await _tokenService.GetTokenAsync(new Uri("https://productstrategy1.aprimo.com/"), _settings.ClientId!, _settings.ClientSecret!);
			if (!tokenResult.Success)
			{
				return BadRequest();
			}
			return Ok(tokenResult.Token);
		}
	}
}
