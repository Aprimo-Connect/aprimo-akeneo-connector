using API.Configuration;
using System.ComponentModel.DataAnnotations;

namespace API.Akeneo
{
	public class AkeneoTenantSettings : ScopedSetting, IValidatable
	{
		public string? ClientId { get; set; }
		public string? ClientSecret { get; set; }
		public string Scopes { get; set; } = "read_channel_localization read_channel_settings openid email profile";

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

			return errors;
		}
	}
}
