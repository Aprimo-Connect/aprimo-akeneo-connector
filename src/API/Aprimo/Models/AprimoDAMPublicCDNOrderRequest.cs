namespace API.Aprimo.Models
{
	public class AprimoDAMPublicCDNOrderRequest
	{
		public string Type => "PublicCDN";

		public List<AprimoDAMPublicCDNOrderTargetRequest>? Targets { get; set; }
	}

	public class AprimoDAMPublicCDNOrderTargetRequest
	{
		public string? RecordId { get; set; }

		public List<string>? AssetTypes { get; set; }
	}
}
