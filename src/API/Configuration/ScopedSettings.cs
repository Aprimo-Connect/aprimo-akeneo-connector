using System.ComponentModel.DataAnnotations;

namespace API.Configuration
{

	public abstract class ScopedSetting
	{
		public string? Scope { get; set; }
	}

	public class ScopedSettings<T> : IValidatable where T : ScopedSetting
	{
		private readonly Dictionary<string, T> _settings = new Dictionary<string, T>();

		public ScopedSettings(IEnumerable<T> settings)
		{
			foreach (var setting in settings)
			{
				if (string.IsNullOrEmpty(setting.Scope)) continue;
				_settings.Add(setting.Scope, setting);
			}
		}

		public T? Get(string scope)
		{
			return _settings.GetValueOrDefault(scope);
		}

		public IEnumerable<ValidationException> Validate()
		{
			var errors = new List<ValidationException>();
			foreach (var setting in _settings.Values)
			{
				if (setting is IValidatable validatable)
				{
					errors.AddRange(validatable.Validate());
				}
			}
			return errors;
		}
	}
}
