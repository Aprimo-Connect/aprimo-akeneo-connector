namespace API.Aprimo.Models
{
	public class AprimoDAMFile : AprimoDAMIdentifiableObject
	{
		public AprimoDAMCollection<AprimoDAMFileVersion>? FileVersions { get; set; }
	}
}
