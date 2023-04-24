using System.Security.Claims;

namespace API.Aprimo
{
	public interface IAprimoUserRepository
	{
		Task<(bool Success, IEnumerable<Claim> Claims)> Authenticate(string username, string password);
	}
}
