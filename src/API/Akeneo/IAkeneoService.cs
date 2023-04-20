namespace API.Akeneo
{
	public interface IAkeneoService
	{
		Task<(bool Success, AkeneoOAuthTokenReponse? TokenResponse)> TryGetGetOAuthToken(Uri baseUri, string code);
	}
}
