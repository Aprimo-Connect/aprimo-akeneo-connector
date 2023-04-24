using System.Text.Json.Serialization;

namespace API.Aprimo
{
	public class AprimoOAuthTokenResponse
	{
		[JsonPropertyName("access_token")]
		public string? AccessToken { get; set; }
	}
}
