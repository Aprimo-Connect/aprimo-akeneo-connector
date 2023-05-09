using System.Text.Json.Serialization;

namespace API.Akeneo
{
	public class AkeneoOAuthTokenReponse
	{
		[JsonPropertyName("access_token")]
		public string? AccessToken { get; set; }


		[JsonPropertyName("token_type")]
		public string? TokenType { get; set; }

		public string? Scope { get; set; }

		[JsonPropertyName("id_token")]
		public string? IDToken { get; set; }
	}
}
