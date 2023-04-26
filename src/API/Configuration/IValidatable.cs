using System.ComponentModel.DataAnnotations;

namespace API.Configuration
{
	public interface IValidatable
	{
		IEnumerable<ValidationException> Validate();
	}
}
