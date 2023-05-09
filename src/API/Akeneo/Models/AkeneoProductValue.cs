namespace API.Akeneo.Models
{
	public class AkeneoProductValue<T> : AkeneoValue<T>
	{
		public string? Scope { get; set; }
	}
}
