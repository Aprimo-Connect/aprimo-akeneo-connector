namespace API.Integration
{
	public class SyncAprimoToAkeneoRequest
	{
		public string AssetFamily { get; set; }

		public string? ProductCode { get; set; }

		public string RecordId { get; set; }

		public string UserId { get; set; }

		public SyncAprimoToAkeneoRequest(string assetFamily, string recordId, string userId)
		{
			AssetFamily = assetFamily;
			RecordId = recordId;
			UserId = userId;
		}
	}
}
