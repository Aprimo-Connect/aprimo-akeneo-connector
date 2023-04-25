namespace API.Aprimo
{
	public interface IAprimoTokenService
	{
		Task<(bool Success, AprimoOAuthTokenResponse? Token)> GetTokenAsync(Uri baseAprimoUri, string clientId, string clientSecret);
	}
}
