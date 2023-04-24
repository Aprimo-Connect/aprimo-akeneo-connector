namespace API.Aprimo
{
	public interface IAprimoTokenService
	{
		Task<(bool Success, AprimoOAuthTokenResponse? token)> GetTokenAsync(Uri baseAprimoUri, string clientId, string clientSecret);
	}
}
