namespace API.Akeneo
{
	public interface IAkeneoTokenService
	{
		Task<bool> IsConfigured();

		Task<string?> GetTokenAsync();

		Task<(bool Success, AkeneoOAuthTokenReponse? TokenResponse)> CompleteOAuthFlow(string code);
	}
}
