namespace API.Akeneo.Models
{
	// https://api.akeneo.com/api-reference.html#patch_asset__code_
	public class AkeneoAssetPatchRequest
	{
		public string? Code { get; set; }

		public Dictionary<string, List<AkeneoValue<string>>>? Values { get; set; }
	}
}
