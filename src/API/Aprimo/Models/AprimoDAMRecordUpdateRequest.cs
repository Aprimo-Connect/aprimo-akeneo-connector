namespace API.Aprimo.Models
{
	public struct AprimoDAMRecordUpdateRequest
	{
		public AprimoDAMRecordFieldsUpdate Fields { get; set; }
	}

	public struct AprimoDAMRecordFieldsUpdate
	{
		public List<AprimoDAMRecordFieldUpdate> AddOrUpdate { get; set; }
	}

	public struct AprimoDAMRecordFieldUpdate
	{
		public string Name { get; set; }

		public List<AprimoDAMFieldLocalizedValue> LocalizedValues { get; set; }
	}
}
