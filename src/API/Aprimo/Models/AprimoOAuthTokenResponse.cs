using System.Text.Json.Serialization;

namespace API.Aprimo.Models
{
	public class AprimoOAuthTokenResponse
	{
		[JsonPropertyName("access_token")]
		public string? AccessToken { get; set; }
	}
}
