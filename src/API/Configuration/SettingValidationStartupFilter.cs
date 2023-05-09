using System.ComponentModel.DataAnnotations;

namespace API.Configuration
{
	public class SettingValidationStartupFilter : IStartupFilter
	{
		readonly IEnumerable<IValidatable> _validatableObjects;

		public SettingValidationStartupFilter(IEnumerable<IValidatable> validatableObjects)
		{
			_validatableObjects = validatableObjects;
		}

		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			var validationErrors = new List<ValidationException>();
			foreach (var validatableObject in _validatableObjects)
			{
				validationErrors.AddRange(validatableObject.Validate());
			}

			if (validationErrors.Any())
			{
				throw new AggregateException(validationErrors);
			}

			return next;
		}
	}
}
