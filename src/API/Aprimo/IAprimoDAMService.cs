using API.Aprimo.Models;

namespace API.Aprimo
{
	public interface IAprimoDAMService
	{
		Task<(bool Success, AprimoDAMPublicCDNOrder? PublicCDNOrder)> GetPublicCDNOrder(string recordId);

		Task<(bool Success, AprimoDAMRecord? record)> GetRecord(string recordId);

		Task<bool> UpdateRecordFields(string recordId, AprimoDAMRecordUpdateRequest recordUpdateRequest);
	}
}
