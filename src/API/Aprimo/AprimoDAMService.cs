using API.Aprimo.Models;
using System.Net;
using System.Text.Json;

namespace API.Aprimo
{
	public class AprimoDAMService : IAprimoDAMService
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly AprimoTenant _tenant;

		public AprimoDAMService(ILogger<AprimoDAMService> logger, HttpClient httpClient, AprimoTenant tenant)
		{
			_logger = logger;
			_httpClient = httpClient;
			_tenant = tenant;
		}

		public async Task<(bool Success, AprimoDAMPublicCDNOrder? PublicCDNOrder)> GetPublicCDNOrder(string recordId)
		{
			var orderUriBuilder = new UriBuilder(_tenant.DAMAPIBaseUri);
			orderUriBuilder.Path = $"/api/core/orders";

			var createOrderRequest = new HttpRequestMessage(HttpMethod.Post, orderUriBuilder.Uri);
			var publicCDNOrderRequest = new AprimoDAMPublicCDNOrderRequest
			{
				Targets = new List<AprimoDAMPublicCDNOrderTargetRequest>
				{
					new AprimoDAMPublicCDNOrderTargetRequest
					{
						RecordId = recordId,
						AssetTypes = new List<string> { "LatestVersionOfMasterFile" }
					}
				}
			};
			createOrderRequest.Content = JsonContent.Create<AprimoDAMPublicCDNOrderRequest>(publicCDNOrderRequest, options: _jsonSerializerOptions);
			var (success, result) = await _httpClient.SendRequestAsyncWithResult<AprimoDAMPublicCDNOrder>(createOrderRequest);
			if (!success || result == null || result.Data == null)
			{
				_logger.LogError("Failed to get order from {url}", orderUriBuilder.Uri);
				return (false, null);
			}

			var order = result.Data;
			if (order == null || string.IsNullOrEmpty(order.Id))
			{
				_logger.LogError("Failed to deserialize response from {url}", orderUriBuilder.Uri);
				return (false, null);
			}

			return (true, order);
		}

		public async Task<(bool Success, AprimoDAMRecord? record)> GetRecord(string recordId)
		{
			var recordUriBuilder = new UriBuilder(_tenant.DAMAPIBaseUri)
			{
				Path = $"/api/core/record/{recordId}"
			};

			var getRecordRequest = new HttpRequestMessage(HttpMethod.Get, recordUriBuilder.Uri);
			getRecordRequest.Headers.Add("select-record", "fields");
			var (success, result) = await _httpClient.SendRequestAsyncWithResult<AprimoDAMRecord>(getRecordRequest);
			if (!success || result == null || result.Data == null)
			{
				_logger.LogError("Failed to get record from {url}", recordUriBuilder.Uri);
				return (false, null);
			}

			var record = result.Data;
			if (record == null || string.IsNullOrEmpty(record.Id))
			{
				_logger.LogError("Failed to deserialize response from {url}", recordUriBuilder.Uri);
				return (false, null);
			}

			return (true, record);
		}

		public async Task<bool> UpdateRecordFields(string recordId, AprimoDAMRecordUpdateRequest recordUpdateRequest)
		{
			var recordUriBuilder = new UriBuilder(_tenant.DAMAPIBaseUri)
			{
				Path = $"/api/core/record/{recordId}"
			};

			var putRequest = new HttpRequestMessage(HttpMethod.Put, recordUriBuilder.Uri)
			{
				Content = JsonContent.Create(recordUpdateRequest, options: _jsonSerializerOptions)
			};
			var response = await _httpClient.SendAsync(putRequest);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogError("Failed to update record at {url}", recordUriBuilder.Uri);
				return false;
			}

			if (response.StatusCode != HttpStatusCode.NoContent)
			{
				_logger.LogError("Failed to update record at {url} with status code {statusCode}. Expected 204.", recordUriBuilder.Uri, response.StatusCode);
				return false;
			}

			return true;
		}
	}
}
