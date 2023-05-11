using API.Akeneo;
using API.Akeneo.Models;
using API.Aprimo;
using API.Aprimo.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace API.Integration
{
	public class AprimoToAkeneoIntegrationService : IAprimoToAkeneoIntegrationService
	{
		private static Regex DYNAMIC_FIELD_LOOKUP = new Regex("^{{(?<field>.*?)}}$", RegexOptions.Compiled | RegexOptions.Singleline);
		private static string RECORD_FIELD_PREFIX = "record.";
		private static string ASSET_FIELD_PREFIX = "asset.";
		private static string PRODUCT_FIELD_PREFIX = "product.";

		private readonly ILogger _logger;
		private readonly IAprimoDAMService _aprimoService;
		private readonly IAkeneoService _akeneoService;
		private readonly AkeneoTenant _akeneoTenant;

		public AprimoToAkeneoIntegrationService(ILogger<AprimoToAkeneoIntegrationService> logger, IAprimoDAMService aprimoService, IAkeneoService akeneoService, AkeneoTenant akeneoTenant)
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
			_logger.LogDebug("Found {x} mappable valueKvp(s) for record {y}", assetMappableValues.Count, record.Id);

			var (dynamicFieldSuccess, _) = PopulateDynamicFields(syncRequest, assetMappableValues);
			if (!dynamicFieldSuccess)
			{
				_logger.LogError("Failed to populate the dynamic fields on the sync request");
				return (false, null);
			}
			if (string.IsNullOrEmpty(syncRequest.AssetFamily))
			{
				_logger.LogInformation("No asset family was specified for record {id}. Nothing to sync.", record.Id);
				return (true, "");
			}

			var fieldMappings = _akeneoTenant.Settings.FieldMappings;

			if (!fieldMappings.ContainsKey(syncRequest.AssetFamily))
			{
				_logger.LogError("The configured field mappings for Akeneo tenant {tenant} do not contain a mapping for asset family {family}", _akeneoTenant.Id, syncRequest.AssetFamily);
				return (false, null);
			}

			var assetFamilyMappingConfiguration = fieldMappings[syncRequest.AssetFamily];
			if (assetFamilyMappingConfiguration == null)
			{
				_logger.LogWarning("No field mappings found for asset family {family}", syncRequest.AssetFamily);
				assetFamilyMappingConfiguration = new Dictionary<string, string>();
			}

			var (cdnSuccess, order) = await _aprimoService.GetPublicCDNOrder(record.Id);
			if (!cdnSuccess || order == null || string.IsNullOrEmpty(order.DeliveredFiles?.First().DeliveredPath ?? ""))
			{
				_logger.LogError("Failed to get public link for record {id}", record.Id);
				return (false, null);
			}
			assetMappableValues[$"{RECORD_FIELD_PREFIX}publicUri"] = new TextValue(order.DeliveredFiles?.First().DeliveredPath ?? "");

			var akeneoValues = new Dictionary<string, List<AkeneoValue<string>>>();
			foreach (var kvp in assetFamilyMappingConfiguration)
			{
				if (!kvp.Key.StartsWith(RECORD_FIELD_PREFIX))
				{
					continue;
				}
				var sourceField = kvp.Key;

				if (!kvp.Value.StartsWith(ASSET_FIELD_PREFIX))
				{
					continue;
				}
				var targetField = kvp.Value.Substring(ASSET_FIELD_PREFIX.Length);

				if (!assetMappableValues.ContainsKey(sourceField))
				{
					continue;
				}

				var value = new AkeneoValue<string>
				{
					Data = assetMappableValues[sourceField].GetValue()
				};
				akeneoValues.Add(targetField, new List<AkeneoValue<string>> { value });
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

			if (string.IsNullOrEmpty(syncRequest.ProductCode))
			{
				return (true, link);
			}

			var productAssetAttribute = assetFamilyMappingConfiguration[AkeneoTenantSettings.ProductAssetAttributeConfigurationKey];
			if (string.IsNullOrEmpty(productAssetAttribute))
			{
				_logger.LogError("Missing product configuration mapping {configKey} for Asset Family {family} for Akeneo tenant {tenant}", AkeneoTenantSettings.ProductAssetAttributeConfigurationKey, syncRequest.AssetFamily, _akeneoTenant.Id);
				return (false, null);
			}

			var (getProductSuccess, product) = await _akeneoService.GetProduct(syncRequest.ProductCode);
			if (!getProductSuccess || product == null)
			{
				_logger.LogError("Failed to load product {code}", syncRequest.ProductCode);
				return (false, null);
			}

			var (associateAssetToProductSuccess, productLink) = await _akeneoService.AddAssetToProduct(akeneoAssetPatchRequest.Code, productAssetAttribute, product);
			if (!associateAssetToProductSuccess || productLink == null)
			{
				_logger.LogError("Failed to associate asset {asset} to product {product}", akeneoAssetPatchRequest.Code, syncRequest.ProductCode);
				return (false, null);
			}

			var productToRecordMappableValues = GetMappableValuesForProduct(product);
			var aprimoRecordFields = new List<AprimoDAMRecordFieldUpdate>();
			foreach (var kvp in assetFamilyMappingConfiguration)
			{
				if (!kvp.Key.StartsWith(PRODUCT_FIELD_PREFIX))
				{
					continue;
				}
				var sourceField = kvp.Key;
				if (!productToRecordMappableValues.ContainsKey(sourceField))
				{
					continue;
				}

				if (!kvp.Value.StartsWith(RECORD_FIELD_PREFIX))
				{
					continue;
				}
				var targetField = kvp.Value.Substring(RECORD_FIELD_PREFIX.Length);

				aprimoRecordFields.Add(new AprimoDAMRecordFieldUpdate
				{
					Name = targetField,
					LocalizedValues = new List<AprimoDAMFieldLocalizedValue>
					{
						new AprimoDAMFieldLocalizedValue
						{
							LanguageId = Guid.Empty.ToString(),
							Value = productToRecordMappableValues[sourceField].GetValue()
						}
					}
				});
			}

			if (aprimoRecordFields.Count <= 0)
			{
				_logger.LogWarning("No fields identified to sync from product to record");
			}
			else
			{
				var recordUpdateRequest = new AprimoDAMRecordUpdateRequest
				{
					Fields = new AprimoDAMRecordFieldsUpdate
					{
						AddOrUpdate = aprimoRecordFields
					}
				};
				var updateRecordSuccess = await _aprimoService.UpdateRecordFields(syncRequest.RecordId, recordUpdateRequest);
				if (!updateRecordSuccess)
				{
					return (false, null);
				}
			}

			return (true, link);
		}

		private Dictionary<string, IMappableValue> GetMappableValuesForRecord(AprimoDAMRecord record)
		{
			var mappableValues = new Dictionary<string, IMappableValue>();

			mappableValues[$"{RECORD_FIELD_PREFIX}syncDate"] = new TextValue(DateTime.UtcNow.ToString("o"));
			mappableValues[$"{RECORD_FIELD_PREFIX}id"] = new TextValue(record.Id!);
			mappableValues[$"{RECORD_FIELD_PREFIX}contentType"] = new TextValue(record.ContentType ?? "");
			mappableValues[$"{RECORD_FIELD_PREFIX}title"] = new TextValue(record.Title ?? "");

			foreach (var field in record.Fields?.Items ?? Array.Empty<AprimoDAMField>())
			{
				switch (field.DataType)
				{
					case "SingleLineText":
						{
							mappableValues[$"{RECORD_FIELD_PREFIX}fields.{field.FieldName}"] = new TextValue(field.LocalizedValues?.FirstOrDefault()?.Value ?? "");
							break;
						}
					case "MultiLineText":
						{
							mappableValues[$"{RECORD_FIELD_PREFIX}fields.{field.FieldName}"] = new TextValue(field.LocalizedValues?.FirstOrDefault()?.Value ?? "");
							break;
						}
						// TODO: Add other field types
						// TODO: Add support for localized values
				}
			}

			return mappableValues;
		}

		private Dictionary<string, IMappableValue> GetMappableValuesForProduct(AkeneoProduct product)
		{
			var mappableValues = new Dictionary<string, IMappableValue>();

			mappableValues[$"{PRODUCT_FIELD_PREFIX}syncDate"] = new TextValue(DateTime.UtcNow.ToString("o"));
			mappableValues[$"{PRODUCT_FIELD_PREFIX}identifier"] = new TextValue(product.Identifier!);

			foreach (var valueKvp in product.Values ?? new Dictionary<string, List<AkeneoProductValue<object>>>())
			{
				var valueName = valueKvp.Key;
				if (valueKvp.Value.Count <= 0)
				{
					continue;
				}

				var value = valueKvp.Value[0];
				if (value.Data is JsonElement jsonData)
				{
					switch (jsonData.ValueKind)
					{
						case JsonValueKind.String:
							{
								mappableValues[$"{PRODUCT_FIELD_PREFIX}values.{valueName}"] = new TextValue(jsonData.GetString() ?? "");
								break;
							}
							// TODO: Add other field types
							// TODO: Add support for localized values
					}
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
