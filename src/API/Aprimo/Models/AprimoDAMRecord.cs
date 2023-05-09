namespace API.Aprimo.Models
{
	public class AprimoDAMRecord : AprimoDAMIdentifiableObject
	{
		public AprimoDAMCollection<AprimoDAMField>? Fields { get; set; }
	}
}
