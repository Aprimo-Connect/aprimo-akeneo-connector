using API.Aprimo.Models;

namespace API.Aprimo
{
	public interface IAprimoService
	{
		Task<(bool Success, AprimoUser? User)> GetCurrentUser();
	}
}
