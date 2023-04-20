﻿namespace API.Configuration
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
			foreach (var validatableObject in _validatableObjects)
			{
				validatableObject.Validate();
			}

			return next;
		}
	}
}
