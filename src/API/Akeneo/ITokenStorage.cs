namespace API.Akeneo
{
	public interface ITokenStorage
	{
		Task<string?> GetTokenAsync();

		Task SetTokenAsync(string token);
	}
}
