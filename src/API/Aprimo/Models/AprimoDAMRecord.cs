namespace API.Aprimo.Models
{
	public class AprimoDAMRecord : AprimoDAMIdentifiableObject
	{
		public AprimoDAMCollection<AprimoDAMFile>? Files { get; set; }
	}
}
