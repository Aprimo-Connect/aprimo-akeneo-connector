namespace API.Akeneo.Models
{
	public class AkeneoProduct
	{
		public string? Identifier { get; set; }

		public Dictionary<string, List<AkeneoProductValue<object>>>? Values { get; set; }
	}
}
