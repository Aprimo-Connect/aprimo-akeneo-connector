using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class AprimoController : ControllerBase
	{
		private readonly ILogger _logger;

		public AprimoController(ILogger<AprimoController> logger)
		{
			_logger = logger;
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
	}
}
