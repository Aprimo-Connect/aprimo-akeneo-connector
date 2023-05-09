using API.Akeneo;
using API.Akeneo.Models;
using API.Aprimo;
using API.Aprimo.Models;
using System.Text.RegularExpressions;

namespace API.Integration
{
	public class AprimoToAkeneoIntegrationService : IAprimoToAkeneoIntegrationService
	{
		private static Regex DYNAMIC_FIELD_LOOKUP = new Regex("^{{(?<field>.*?)}}$", RegexOptions.Compiled | RegexOptions.Singleline);

		private readonly ILogger _logger;
		private readonly IAprimoService _aprimoService;
		private readonly IAkeneoService _akeneoService;
		private readonly AkeneoTenant _akeneoTenant;

		public AprimoToAkeneoIntegrationService(ILogger<AprimoToAkeneoIntegrationService> logger, IAprimoService aprimoService, IAkeneoService akeneoService, AkeneoTenant akeneoTenant)
		{
			_logger = logger;
			_aprimoService = aprimoService;
			_akeneoService = akeneoService;
			_akeneoTenant = akeneoTenant;
		}

		public async Task<(bool Success, string? Result)> SendAprimoDAMRecordToAkeneo(SyncAprimoToAkeneoRequest syncRequest)
		{
			var (success, record) = await _aprimoService.GetRecord(syncRequest.RecordId);
			if (!success || record == null || record.Id == null)
			{
				_logger.LogError("Failed to load record {id}", syncRequest.RecordId);
				return (false, null);
			}

			var assetMappableValues = GetMappableValuesForRecord(record);
			_logger.LogDebug("Found {x} mappable value(s) for record {y}", assetMappableValues.Count, record.Id);

			var (dynamicFieldSuccess, _) = PopulateDynamicFields(syncRequest, assetMappableValues);
			if (!dynamicFieldSuccess)
			{
				_logger.LogError("Failed to populate the dynamic fields on the sync request");
				return (false, null);
			}

			var fieldMappings = _akeneoTenant.Settings.FieldMappings;

			if (!fieldMappings.ContainsKey(syncRequest.AssetFamily))
			{
				_logger.LogError("The configured field mappings for Akeneo tenant {tenant} do not contain a mapping for asset family {family}", _akeneoTenant.Id, syncRequest.AssetFamily);
				return (false, null);
			}

			var mapping = fieldMappings[syncRequest.AssetFamily];
			if (mapping == null)
			{
				_logger.LogWarning("No field mappings found for asset family {family}", syncRequest.AssetFamily);
				mapping = new Dictionary<string, string>();
			}

			var (cdnSuccess, order) = await _aprimoService.GetPublicCDNOrder(record.Id);
			if (!cdnSuccess || order == null || string.IsNullOrEmpty(order.DeliveredFiles?.First().DeliveredPath ?? ""))
			{
				_logger.LogError("Failed to get public link for record {id}", record.Id);
				return (false, null);
			}
			assetMappableValues["record.publicUri"] = new TextValue(order.DeliveredFiles?.First().DeliveredPath ?? "");

			var akeneoValues = new Dictionary<string, List<AkeneoValue<string>>>();
			foreach (var kvp in mapping)
			{
				if (assetMappableValues.ContainsKey(kvp.Key))
				{
					var value = new AkeneoValue<string>
					{
						Data = assetMappableValues[kvp.Key].GetValue()
					};
					akeneoValues.Add(kvp.Value, new List<AkeneoValue<string>> { value });
				}
			}

			var akeneoAssetPatchRequest = new AkeneoAssetPatchRequest
			{
				Code = record.Id,
				Values = akeneoValues
			};
			var (createOrUpdateSuccess, link) = await _akeneoService.CreateOrUpdateAsset(syncRequest.AssetFamily, akeneoAssetPatchRequest);

			if (!createOrUpdateSuccess || link == null)
			{
				return (false, null);
			}

			if (!string.IsNullOrEmpty(syncRequest.ProductCode))
			{
				var (associateAssetToProductSuccess, productLink) = await _akeneoService.AddAssetToProduct(akeneoAssetPatchRequest.Code, syncRequest.ProductCode);
				if (!associateAssetToProductSuccess || productLink == null)
				{
					_logger.LogError("Failed to associate asset {asset} to product {product}", akeneoAssetPatchRequest.Code, syncRequest.ProductCode);
					return (false, null);
				}
			}

			return (true, null);
		}

		private Dictionary<string, IMappableValue> GetMappableValuesForRecord(AprimoDAMRecord record)
		{
			var mappableValues = new Dictionary<string, IMappableValue>();

			mappableValues["record.id"] = new TextValue(record.Id!);

			foreach (var field in record.Fields?.Items ?? Array.Empty<AprimoDAMField>())
			{
				switch (field.DataType)
				{
					case "SingleLineText":
						{
							mappableValues[$"record.fields.{field.FieldName}"] = new TextValue(field.LocalizedValues?.FirstOrDefault()?.Value ?? "");
							break;
						}
						// TODO: Add other field types
				}
			}

			return mappableValues;
		}

		private (bool Success, SyncAprimoToAkeneoRequest SyncRequest) PopulateDynamicFields(SyncAprimoToAkeneoRequest syncRequest, Dictionary<string, IMappableValue> assetMappableValues)
		{
			var assetFamilyLookup = DYNAMIC_FIELD_LOOKUP.Match(syncRequest.AssetFamily);
			if (assetFamilyLookup.Success)
			{
				var fieldLookup = assetFamilyLookup.Groups["field"].Value;
				if (!assetMappableValues.ContainsKey(fieldLookup))
				{
					return (false, syncRequest);
				}

				syncRequest.AssetFamily = assetMappableValues[fieldLookup].GetValue();
			}
			var productCodeLookup = DYNAMIC_FIELD_LOOKUP.Match(syncRequest.ProductCode ?? "");
			if (productCodeLookup.Success)
			{
				var fieldLookup = productCodeLookup.Groups["field"].Value;
				if (!assetMappableValues.ContainsKey(fieldLookup))
				{
					return (false, syncRequest);
				}

				syncRequest.ProductCode = assetMappableValues[fieldLookup].GetValue();
			}

			return (true, syncRequest);
		}
	}
}
