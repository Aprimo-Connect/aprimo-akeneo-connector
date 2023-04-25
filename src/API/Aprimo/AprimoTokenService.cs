using System.Text.Json;

namespace API.Aprimo
{
	public class AprimoTokenService : IAprimoTokenService
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;

		public AprimoTokenService(ILogger<AprimoTokenService> logger, HttpClient httpClient)
		{
			_logger = logger;
			_httpClient = httpClient;
		}

		public async Task<(bool Success, AprimoOAuthTokenResponse? Token)> GetTokenAsync(Uri baseAprimoUri, string clientId, string clientSecret)
		{
			var tokenUriBuilder = new UriBuilder(baseAprimoUri);
			tokenUriBuilder.Path = "/login/connect/token";

			var response = await _httpClient.PostAsync(tokenUriBuilder.Uri, GetOAuthTokenRequestBody(clientId, clientSecret));
			var tokenResponse = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Failed to get token from {url}: {status}", tokenUriBuilder.Uri, response.StatusCode);
				return (false, null);
			}

			var token = JsonSerializer.Deserialize<AprimoOAuthTokenResponse>(tokenResponse);
			if (token == null)
			{
				_logger.LogError("Failed to deserialize response from {url}: {response}", tokenUriBuilder.Uri, tokenResponse);
				return (false, null);
			}

			return (true, token);
		}

		private HttpContent GetOAuthTokenRequestBody(string clientId, string clientSecret)
		{
			var postData = new[]
			{
				new KeyValuePair<string, string?>("grant_type", "client_credentials"),
				new KeyValuePair<string, string?>("scope", "api"),
				new KeyValuePair<string, string?>("client_id", clientId),
				new KeyValuePair<string, string?>("client_secret", clientSecret),
			};

			return new FormUrlEncodedContent(postData);
		}
	}
}
