namespace API.Akeneo
{
	public interface IAkeneoService
	{
		Task<bool> IsConfigured();

		Task<(bool Success, AkeneoOAuthTokenReponse? TokenResponse)> CompleteOAuthFlow(string code);
	}
}
