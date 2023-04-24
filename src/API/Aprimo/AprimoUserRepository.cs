using API.Configuration;
using System.Security.Claims;

namespace API.Aprimo
{
	public class AprimoUserRepository : IAprimoUserRepository
	{
		private readonly IConfiguration _config;
		private readonly ILogger _logger;
		private readonly AprimoSettings _settings;

		public AprimoUserRepository(ILogger<AprimoUserRepository> logger, IConfiguration config, AprimoSettings aprimoSettings)
		{
			_logger = logger;
			_config = config;
			_settings = aprimoSettings;
		}

		public Task<(bool Success, IEnumerable<Claim> Claims)> Authenticate(string username, string password)
		{
			if (_settings.Users == null || !_settings.Users.TryGetValue(username, out string? expectedPassword))
			{
				(bool, IEnumerable<Claim>) failedResult = (false, new List<Claim>());
				return Task.FromResult(failedResult);
			}

			(bool, IEnumerable<Claim>) successResult = (expectedPassword.Equals(password), new List<Claim>
			{
				new Claim(ClaimTypes.Name, username),
			});

			return Task.FromResult(successResult);
		}
	}
}
