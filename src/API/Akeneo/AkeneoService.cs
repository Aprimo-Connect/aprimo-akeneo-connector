using API.Akeneo.Models;
using System.Text.Json;

namespace API.Akeneo
{
	public class AkeneoService : IAkeneoService
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly AkeneoTenant _tenant;
		private readonly Uri _baseUri;

		public AkeneoService(ILogger<AkeneoService> logger, HttpClient httpClient, AkeneoTenant tenant)
		{
			_logger = logger;
			_httpClient = httpClient;
			_tenant = tenant;

			_baseUri = new Uri($"https://{_tenant.Id}");
		}

		public async Task<(bool Success, AkeneoProduct? Product)> GetProduct(string productCode)
		{
			var getProductUriBuilder = new UriBuilder(_baseUri)
			{
				Path = $"/api/rest/v1/products/{productCode}"
			};

			var getRequest = new HttpRequestMessage(HttpMethod.Get, getProductUriBuilder.Uri);
			var (success, result) = await _httpClient.SendRequestAsyncWithResult<AkeneoProduct>(getRequest);
			if (!success || result == null || result.Data == null)
			{
				_logger.LogError("Failed to product with code {code} from {url}", productCode, getProductUriBuilder.Uri);
				return (false, null);
			}

			var product = result.Data;
			if (product == null || string.IsNullOrEmpty(product.Identifier))
			{
				return (false, null);
			}

			return (true, result.Data);
		}

		public async Task<(bool Success, string? Product)> AddAssetToProduct(string assetCode, string productAttributeName, AkeneoProduct product)
		{
			var newAssetData = GetAssetsPatchData(assetCode, productAttributeName, product);
			if (newAssetData.Length == 0)
			{
				return (false, null);
			}

			var values = new Dictionary<string, List<AkeneoProductValue<object>>>();
			values[productAttributeName] = new List<AkeneoProductValue<object>>
			{
				new AkeneoProductValue<object>
				{
					Data = newAssetData
				}
			};
			var productPatchRequestData = new AkeneoProductPatchRequest
			{
				Identifier = product.Identifier,
				Values = values
			};
			var updateProductUriBuilder = new UriBuilder(_baseUri)
			{
				Path = $"/api/rest/v1/products/{product.Identifier}"
			};
			var patchRequest = new HttpRequestMessage(HttpMethod.Patch, updateProductUriBuilder.Uri)
			{
				Content = JsonContent.Create<AkeneoProductPatchRequest>(productPatchRequestData, options: _jsonSerializerOptions)
			};
			var response = await _httpClient.SendAsync(patchRequest);
			if (!response.IsSuccessStatusCode)
			{
				return (false, null);
			}

			if (response.Headers.Location == null)
			{
				return (false, null);
			}

			return (true, response.Headers.Location.ToString());
		}

		public async Task<(bool Success, string? Asset)> CreateOrUpdateAsset(string assetFamilyCode, AkeneoAssetPatchRequest assetPatchRequest)
		{
			var createAssetRequestUriBuilder = new UriBuilder(_baseUri)
			{
				Path = $"/api/rest/v1/asset-families/{assetFamilyCode}/assets/{assetPatchRequest.Code}"
			};

			var patchRequest = new HttpRequestMessage(HttpMethod.Patch, createAssetRequestUriBuilder.Uri)
			{
				Content = JsonContent.Create<AkeneoAssetPatchRequest>(assetPatchRequest, options: _jsonSerializerOptions)
			};
			var response = await _httpClient.SendAsync(patchRequest);
			if (!response.IsSuccessStatusCode)
			{
				return (false, null);
			}

			if (response.Headers.Location == null)
			{
				return (false, null);
			}

			return (true, response.Headers.Location.ToString());
		}

		private string[] GetAssetsPatchData(string assetCode, string productAttributeName, AkeneoProduct product)
		{
			var newAssetData = new string[] { assetCode };
			// If there are existing assets associated to this product, we need to include them in the PATCH request
			var existingProductAttributes = product.Values ?? new Dictionary<string, List<AkeneoProductValue<object>>>();
			if (existingProductAttributes.ContainsKey(productAttributeName))
			{
				var existingAssetData = existingProductAttributes[productAttributeName];
				if (existingAssetData.Count > 0)
				{
					var existingValue = existingAssetData[0];
					if (existingValue.Data is JsonElement element)
					{
						if (element.ValueKind.Equals(JsonValueKind.Array))
						{
							var data = element.EnumerateArray().Select(x => x.GetString() ?? "").Where(x => !string.IsNullOrEmpty(x));
							newAssetData = data.Concat(newAssetData).Distinct().ToArray();
						}
						else
						{
							_logger.LogCritical("Unexpected value kind {kind} for product attribute {attributeName}", element.ValueKind, productAttributeName);
							return Array.Empty<string>();
						}
					}
				}
			}

			return newAssetData;
		}
	}
}
