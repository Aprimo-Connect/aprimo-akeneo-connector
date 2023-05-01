using API.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace API.Akeneo
{
	public class AkeneoService : IAkeneoService
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
		};
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly AkeneoTenant _tenant;
		private readonly RandomNumberGenerator _randomNumberGenerator;
		private readonly ITokenStorage _tokenStorage;
		private readonly Uri _baseUri;
		private readonly string _tokenStorageKey;

		public AkeneoService(ILogger<AkeneoService> logger, HttpClient httpClient, AkeneoTenant tenant, ITokenStorage tokenStorage)
		{
			_logger = logger;
			_httpClient = httpClient;
			_tenant = tenant;
			_randomNumberGenerator = RandomNumberGenerator.Create();
			_tokenStorage = tokenStorage;

			_baseUri = new Uri($"https://{_tenant.Id}");
			_tokenStorageKey = $"akeneo_{_tenant.Id}";
		}

		public async Task<bool> IsConfigured()
		{
			var token = await _tokenStorage.GetTokenAsync(_tokenStorageKey);

			return !string.IsNullOrEmpty(token);
		}

		public async Task<(bool Success, AkeneoOAuthTokenReponse? TokenResponse)> CompleteOAuthFlow(string code)
		{
			var tokenUrlBuilder = new UriBuilder(_baseUri)
			{
				Path = "/connect/apps/v1/oauth2/token"
			};


			var postRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrlBuilder.Uri);
			postRequest.Content = CreateOAuthRequestContent(code);

			var (success, result) = await _httpClient.SendRequestAsyncWithResult<AkeneoOAuthTokenReponse>(postRequest);
			if (!success || result == null)
			{
				_logger.LogError("Failed to make request to {url}", tokenUrlBuilder.Uri);
				return (false, null);
			}

			if (!success && result.Response != null)
			{
				_logger.LogError("Failed to make request to {url} with status {status}", tokenUrlBuilder.Uri, result.Response.StatusCode);
				return (false, null);
			}

			if (result.Data == null)
			{
				_logger.LogError("Failed to deserialize response from {url}", tokenUrlBuilder.Uri);
				return (false, null);
			}

			await _tokenStorage.SetTokenAsync(_tokenStorageKey, result.Data.AccessToken!);

			return (true, result.Data);
		}

		private HttpContent CreateOAuthRequestContent(string code)
		{
			var (codeIdentifier, codeChallenge) = GetOAuthCodeChallenge();

			var postData = new[]
			{
					new KeyValuePair<string, string?>("grant_type", "authorization_code"),
					new KeyValuePair<string, string?>("code", code),
					new KeyValuePair<string, string?>("client_id", _tenant.Settings.ClientId),
					new KeyValuePair<string, string?>("code_identifier", codeIdentifier),
					new KeyValuePair<string, string?>("code_challenge", codeChallenge),
				};

			var content = new FormUrlEncodedContent(postData);

			return content;
		}

		private (string codeIdentifier, string codeChallenge) GetOAuthCodeChallenge()
		{
			var bytes = new byte[30];
			_randomNumberGenerator.GetBytes(bytes);
			var code_identifier = Convert.ToHexString(bytes).ToLower();

			using (var sha256 = SHA256.Create())
			{
				var challengeBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes($"{code_identifier}{_tenant.Settings.ClientSecret}"));
				var code_challenge = Convert.ToHexString(challengeBytes).ToLower();
				return (code_identifier, code_challenge);
			}
		}
	}
}
