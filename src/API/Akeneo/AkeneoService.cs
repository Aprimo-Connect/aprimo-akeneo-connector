using API.Configuration;
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
		private readonly AkeneoSettings _settings;
		private readonly RandomNumberGenerator _randomNumberGenerator;

		public AkeneoService(ILogger<AkeneoService> logger, HttpClient httpClient, AkeneoSettings settings)
		{
			_logger = logger;
			_httpClient = httpClient;
			_settings = settings;
			_randomNumberGenerator = RandomNumberGenerator.Create();
		}

		public async Task<(bool Success, AkeneoOAuthTokenReponse? TokenResponse)> TryGetGetOAuthToken(Uri baseUri, string code)
		{
			var tokenUrlBuilder = new UriBuilder(baseUri)
			{
				Path = "/connect/apps/v1/oauth2/token"
			};

			var response = await _httpClient.PostAsync(tokenUrlBuilder.Uri, CreateOAuthRequestContent(code));
			var tokenResponse = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Failed to make request to {url}: {status}", tokenUrlBuilder.Uri, response.StatusCode);
				return (false, null);
			}

			return (true, JsonSerializer.Deserialize<AkeneoOAuthTokenReponse>(tokenResponse, _jsonSerializerOptions));
		}

		private HttpContent CreateOAuthRequestContent(string code)
		{
			var (codeIdentifier, codeChallenge) = GetOAuthCodeChallenge();

			var postData = new[]
			{
				new KeyValuePair<string, string?>("grant_type", "authorization_code"),
				new KeyValuePair<string, string?>("code", code),
				new KeyValuePair<string, string?>("client_id", _settings.ClientId),
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
				var challengeBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes($"{code_identifier}{_settings.ClientSecret}"));
				var code_challenge = Convert.ToHexString(challengeBytes).ToLower();
				return (code_identifier, code_challenge);
			}
		}
	}
}
