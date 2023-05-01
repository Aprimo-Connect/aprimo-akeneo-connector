namespace API.Aprimo.Models
{
	public class AprimoDAMFileVersion : AprimoDAMIdentifiableObject
	{
		public AprimoDAMCollection<AprimoDAMPublicLink>? PublicLinks { get; set; }
	}
}
