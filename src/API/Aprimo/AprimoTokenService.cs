using API.Aprimo.Models;
using API.Tokens;

namespace API.Aprimo
{
	public class AprimoTokenService : IAprimoTokenService
	{
		private readonly ILogger _logger;
		private readonly ITokenStorage _tokenStorage;
		private readonly HttpClient _httpClient;
		private readonly AprimoTenant _tenant;
		private string _tokenStorageKey => $"aprimo_{_tenant.Id}";

		public AprimoTokenService(ILogger<AprimoTokenService> logger, ITokenStorage tokenStorage, HttpClient httpClient, AprimoTenant tenant)
		{
			_logger = logger;
			_tokenStorage = tokenStorage;
			_httpClient = httpClient;
			_tenant = tenant;
		}

		public async Task<(bool Success, string? Token)> TryGetTokenAsync(bool useCache = true)
		{
			if (useCache)
			{
				var token = await _tokenStorage.GetTokenAsync(_tokenStorageKey);
				if (!string.IsNullOrEmpty(token))
				{
					return (true, token);
				}
			}

			var tokenUriBuilder = new UriBuilder(_tenant.OAuthBaseUri);
			tokenUriBuilder.Path = "/login/connect/token";

			var postRequest = new HttpRequestMessage(HttpMethod.Post, tokenUriBuilder.Uri);
			postRequest.Content = GetOAuthTokenRequestBody(_tenant.Settings.ClientId!, _tenant.Settings.ClientSecret!);

			var (success, result) = await _httpClient.SendRequestAsyncWithResult<AprimoOAuthTokenResponse>(postRequest);
			if (!success || result == null || result.Data == null)
			{
				_logger.LogError("Failed to get token from {url}", tokenUriBuilder.Uri);
				return (false, null);
			}

			if (string.IsNullOrEmpty(result.Data.AccessToken))
			{
				_logger.LogError("Failed to deserialize response from {url}", tokenUriBuilder.Uri);
				return (false, null);
			}

			await _tokenStorage.SetTokenAsync(_tokenStorageKey, result.Data.AccessToken);

			return (true, result.Data.AccessToken);
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

		public async Task<bool> ClearTokenAsync()
		{
			await _tokenStorage.SetTokenAsync(_tokenStorageKey, "");

			return true;
		}
	}
}
