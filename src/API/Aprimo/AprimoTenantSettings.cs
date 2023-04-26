using API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace API.Aprimo
{
	public class AprimoTenantSettings : ScopedSetting, IValidatable
	{
		public string? ClientId { get; set; }

		public string? ClientSecret { get; set; }

		public string? BasicAuthPassword { get; set; }

		public string HMACSecret { get; set; } = string.Empty;

		public IEnumerable<ValidationException> Validate()
		{
			var errors = new List<ValidationException>();

			if (string.IsNullOrEmpty(ClientId))
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AprimoTenantSettings)}:{nameof(ClientId)}"));
			}

			if (string.IsNullOrEmpty(ClientSecret))
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AprimoTenantSettings)}:{nameof(ClientSecret)}"));
			}

			if (string.IsNullOrEmpty(BasicAuthPassword))
			{
				errors.Add(new ValidationException($"Missing required configuration value: {nameof(AprimoTenantSettings)}:{nameof(BasicAuthPassword)}"));
			}

			return errors;
		}
	}
}
