using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace API.Aprimo
{
	public class AprimoRuleAuthenticationHandler : AuthenticationHandler<AprimoRuleAuthenticationHandlerOptions>
	{
		private readonly IAprimoUserRepository _userRepository;

		public AprimoRuleAuthenticationHandler(
			IOptionsMonitor<AprimoRuleAuthenticationHandlerOptions> optionsMonitor,
			ILoggerFactory loggerFactory,
			UrlEncoder encoder,
			ISystemClock clock,
			IAprimoUserRepository userRepository)
			: base(optionsMonitor, loggerFactory, encoder, clock)
		{
			_userRepository = userRepository;
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			var (success, claims) = await Authenticate();
			if (!success)
			{
				Response.StatusCode = 401;
				Response.Headers.Add("WWW-Authenticate", "Basic realm=\"AprimoAkeneoConnector\"");
				return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
			}

			var identity = new ClaimsIdentity(claims, "Basic");
			var claimsPrincipal = new ClaimsPrincipal(identity);
			return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
		}

		private async Task<(bool Success, IEnumerable<Claim>? Claims)> Authenticate()
		{
			var failCase = (false, (IEnumerable<Claim>?)null);
			var authorizationHeader = Request.Headers["Authorization"].ToString();
			if (authorizationHeader == null)
			{
				return failCase;
			}

			if (!authorizationHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
			{
				return failCase;
			}

			var token = authorizationHeader.Substring("Basic ".Length).Trim();
			if (string.IsNullOrEmpty(token))
			{
				return failCase;
			}

			string credentialsAsEncodedString;
			try
			{
				credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
			}
			catch (FormatException)
			{
				return failCase;
			}

			var credentials = credentialsAsEncodedString.Split(':', 2);
			if (credentials.Length != 2)
			{
				return failCase;
			}

			return await _userRepository.Authenticate(credentials[0], credentials[1]);
		}
	}

	public class AprimoRuleAuthenticationHandlerOptions : AuthenticationSchemeOptions
	{

	}
}
