using API.Aprimo.Models;

namespace API.Aprimo
{
	public interface IAprimoService
	{
		Task<(bool Success, AprimoDAMPublicCDNOrder? PublicCDNOrder)> GetPublicCDNOrder(string recordId);

		Task<(bool Success, AprimoDAMRecord? record)> GetRecord(string recordId);
	}
}
