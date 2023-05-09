namespace API.Akeneo.Models
{
	public class AkeneoProductPatchRequest
	{
		public string? Identifier { get; set; }

		public Dictionary<string, List<AkeneoProductValue<string[]>>>? Values { get; set; }
	}
}
