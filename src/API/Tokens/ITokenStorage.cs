namespace API.Tokens
{
	public interface ITokenStorage
	{
		Task<string?> GetTokenAsync(string id);

		Task SetTokenAsync(string id, string token);
	}
}
