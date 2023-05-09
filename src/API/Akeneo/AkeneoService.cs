using API.Akeneo.Models;
using API.Tokens;
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

		public AkeneoService(ILogger<AkeneoService> logger, HttpClient httpClient, AkeneoTenant tenant, ITokenStorage tokenStorage)
		{
			_logger = logger;
			_httpClient = httpClient;
			_tenant = tenant;

			_baseUri = new Uri($"https://{_tenant.Id}");
		}

		public async Task<(bool Success, string? Product)> AddAssetToProduct(string assetCode, string productCode)
		{
			var updateProductUriBuilder = new UriBuilder(_baseUri)
			{
				Path = $"/api/rest/v1/products/{productCode}"
			};

			var productPatchRequest = new AkeneoProductPatchRequest
			{
				Identifier = productCode,
				Values = new Dictionary<string, List<AkeneoProductValue<string[]>>>
				{
					{
						"Asset_Collection", new List<AkeneoProductValue<string[]>>
						{
							new AkeneoProductValue<string[]>
							{
								Data = new string[] { assetCode }
							}
						}
					}
				}
			};

			var patchRequest = new HttpRequestMessage(HttpMethod.Patch, updateProductUriBuilder.Uri);
			patchRequest.Content = JsonContent.Create<AkeneoProductPatchRequest>(productPatchRequest, options: _jsonSerializerOptions);
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

			var patchRequest = new HttpRequestMessage(HttpMethod.Patch, createAssetRequestUriBuilder.Uri);
			patchRequest.Content = JsonContent.Create<AkeneoAssetPatchRequest>(assetPatchRequest, options: _jsonSerializerOptions);
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
	}
}
