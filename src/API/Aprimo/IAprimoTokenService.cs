namespace API.Aprimo
{
	public interface IAprimoTokenService
	{
		Task<(bool Success, string? Token)> TryGetTokenAsync(bool useCache = true);

		Task<bool> ClearTokenAsync();
	}
}
