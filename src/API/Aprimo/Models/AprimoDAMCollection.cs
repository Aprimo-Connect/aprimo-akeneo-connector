namespace API.Aprimo.Models
{
	public class AprimoDAMCollection<T>
	{
		public T[] Items { get; set; } = Array.Empty<T>();
	}
}
