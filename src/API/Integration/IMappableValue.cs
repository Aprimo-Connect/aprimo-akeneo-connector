namespace API.Integration
{
	public interface IMappableValue
	{
		string GetValue();
	}

	public class TextValue : IMappableValue
	{
		public string Value { get; set; }

		public TextValue(string value)
		{
			Value = value;
		}

		public string GetValue()
		{
			return Value;
		}
	}

	public class LocalizedTextValue : IMappableValue
	{
		public string LocaleId { get; set; }

		public string LocalizedValue { get; set; }

		public LocalizedTextValue(string localeId, string localizedValue)
		{
			LocaleId = localeId;
			LocalizedValue = localizedValue;
		}

		public string GetValue()
		{
			return LocalizedValue;
		}
	}
}
