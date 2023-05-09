namespace API.Integration
{
	public interface IAprimoToAkeneoIntegrationService
	{
		Task<(bool Success, string? Result)> SendAprimoDAMRecordToAkeneo(SyncAprimoToAkeneoRequest syncRequest);
	}
}
