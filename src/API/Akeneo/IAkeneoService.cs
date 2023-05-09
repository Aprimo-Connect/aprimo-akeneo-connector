using API.Akeneo.Models;

namespace API.Akeneo
{
	public interface IAkeneoService
	{
		Task<(bool Success, string? Asset)> CreateOrUpdateAsset(string assetFamilyCode, AkeneoAssetPatchRequest assetPatchRequest);

		Task<(bool Success, string? Product)> AddAssetToProduct(string assetCode, string productCode);
	}
}
