using System.Net.Http.Headers;

namespace API.Akeneo
{
	public class AkeneoTokenAuthHeaderHandler : DelegatingHandler
	{
		private readonly IAkeneoTokenService _tokenService;
		public AkeneoTokenAuthHeaderHandler(IAkeneoTokenService tokenService)
		{
			_tokenService = tokenService;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var token = await _tokenService.GetTokenAsync();
			if (string.IsNullOrEmpty(token))
			{
				throw new Exception($"Failed to retrieve token while making request to {request.RequestUri}.");
			}

			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
