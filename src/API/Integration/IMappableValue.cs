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
}
