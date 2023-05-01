using API.Aprimo.Models;

namespace API.Aprimo
{
	public class AprimoService : IAprimoService
	{
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly AprimoTenant _tenant;

		public AprimoService(ILogger<AprimoService> logger, HttpClient httpClient, AprimoTenant tenant)
		{
			_logger = logger;
			_httpClient = httpClient;
			_tenant = tenant;
		}

		public async Task<(bool Success, AprimoDAMRecord? record)> GetRecord(string recordId)
		{
			var recordUriBuilder = new UriBuilder(_tenant.DAMBaseUri);
			recordUriBuilder.Path = $"/api/core/record/{recordId}";

			var getRecordRequest = new HttpRequestMessage(HttpMethod.Get, recordUriBuilder.Uri);
			getRecordRequest.Headers.Add("select-record", "files");
			getRecordRequest.Headers.Add("select-file", "fileversions");
			getRecordRequest.Headers.Add("select-fileversion", "publiclinks");
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
	}
}
