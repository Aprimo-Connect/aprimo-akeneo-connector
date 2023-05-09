namespace API.Akeneo.Models
{
	public class AkeneoValue<T>
	{
		public string? Locale { get; set; }

		public string? Channel { get; set; }

		public T? Data { get; set; }
	}
}
