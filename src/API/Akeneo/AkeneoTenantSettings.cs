using API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace API.Akeneo
{
	public class AkeneoTenantSettings : ScopedSetting, IValidatable
	{
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string Scopes { get; set; } = "write_products write_assets read_asset_families";
		public Dictionary<string, Dictionary<string, string>> FieldMappings { get; set; } = new Dictionary<string, Dictionary<string, string>>();

		public IEnumerable<ValidationException> Validate()
		{
			var errors = new List<ValidationException>();

			if (string.IsNullOrEmpty(ClientId))
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AkeneoTenantSettings)}:{nameof(ClientId)}"));
			}
			if (string.IsNullOrEmpty(ClientSecret))
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AkeneoTenantSettings)}:{nameof(ClientSecret)}"));
			}
			if (string.IsNullOrEmpty(Scopes))
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AkeneoTenantSettings)}:{nameof(Scopes)}"));
			}
			if (FieldMappings == null || FieldMappings.Count <= 0)
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AkeneoTenantSettings)}:{nameof(FieldMappings)}"));
			}

			return errors;
		}
	}
}
