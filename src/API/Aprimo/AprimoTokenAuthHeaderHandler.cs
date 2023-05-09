using System.Net;
using System.Net.Http.Headers;

namespace API.Aprimo
{
	public class AprimoTokenAuthHeaderHandler : DelegatingHandler
	{
		private readonly IAprimoTokenService _tokenService;
		public AprimoTokenAuthHeaderHandler(IAprimoTokenService tokenService)
		{
			_tokenService = tokenService;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var (success, token) = await _tokenService.TryGetTokenAsync();
			if (!success || token == null)
			{
				throw new Exception($"Failed to retrieve token while making request to {request.RequestUri}.");
			}

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			var response = await base.SendAsync(request, cancellationToken);
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				// Force a token refresh on the next request.
				await _tokenService.ClearTokenAsync();
			}

			return response;
		}
	}
}
