namespace API.Aprimo.Models
{
	public class AprimoDAMField : AprimoDAMIdentifiableObject
	{
		public string? DataType { get; set; }

		public string? FieldName { get; set; }

		public string? Label { get; set; }

		public IEnumerable<AprimoDAMFieldLocalizedValue>? LocalizedValues { get; set; }
	}
}
