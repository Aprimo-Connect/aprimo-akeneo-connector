namespace API.Integration
{
	public class SyncAprimoToAkeneoRequest
	{
		public string AssetFamily { get; set; }

		public string? ProductCode { get; set; }

		public string RecordId { get; set; }

		public SyncAprimoToAkeneoRequest(string assetFamily, string recordId)
		{
			AssetFamily = assetFamily;
			RecordId = recordId;
		}
	}
}
