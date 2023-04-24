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
			var authorizationHeader = Request.Headers["Authorization"].ToString();
			if (authorizationHeader != null && authorizationHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
			{
				var token = authorizationHeader.Substring("Basic ".Length).Trim();
				var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
				var credentials = credentialsAsEncodedString.Split(':');
				var authResult = await _userRepository.Authenticate(credentials[0], credentials[1]);
				if (authResult.Success)
				{
					var identity = new ClaimsIdentity(authResult.Claims, "Basic");
					var claimsPrincipal = new ClaimsPrincipal(identity);
					return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
				}
			}

			Response.StatusCode = 401;
			Response.Headers.Add("WWW-Authenticate", "Basic realm=\"AprimoAkeneoConnector\"");
			return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
		}
	}

	public class AprimoRuleAuthenticationHandlerOptions : AuthenticationSchemeOptions
	{

	}
}
