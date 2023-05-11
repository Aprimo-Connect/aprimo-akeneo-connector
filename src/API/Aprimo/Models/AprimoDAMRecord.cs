namespace API.Aprimo.Models
{
	public class AprimoDAMRecord : AprimoDAMIdentifiableObject
	{
		public string? ContentType { get; set; }

		public string? Title { get; set; }

		public AprimoDAMCollection<AprimoDAMField>? Fields { get; set; }
	}
}
