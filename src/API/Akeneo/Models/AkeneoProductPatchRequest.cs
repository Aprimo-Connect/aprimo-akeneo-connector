﻿namespace API.Akeneo.Models
{
	public class AkeneoProductPatchRequest
	{
		public string? Identifier { get; set; }

		public Dictionary<string, List<AkeneoProductValue<object>>>? Values { get; set; }
	}
}
