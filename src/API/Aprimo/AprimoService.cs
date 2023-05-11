using API.Aprimo.Models;
using System.Text.Json;

namespace API.Aprimo
{
	public class AprimoService : IAprimoService
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		private readonly ILogger _logger;
		private readonly HttpClient _httpClient;
		private readonly AprimoTenant _tenant;

		public AprimoService(ILogger<AprimoService> logger, HttpClient httpClient, AprimoTenant tenant)
		{
			_logger = logger;
			_httpClient = httpClient;
			_tenant = tenant;
		}

		public async Task<(bool Success, AprimoUser? User)> GetCurrentUser()
		{
			var currentUserUriBuilder = new UriBuilder(_tenant.APIBaseUri);
			currentUserUriBuilder.Path = $"/api/users/me";

			var getRequest = new HttpRequestMessage(HttpMethod.Get, currentUserUriBuilder.Uri);
			var (success, result) = await _httpClient.SendRequestAsyncWithResult<AprimoUser>(getRequest);
			if (!success || result == null)
			{
				_logger.LogError("Failed to get user from {url}", currentUserUriBuilder.Uri);
				return (false, null);
			}

			var user = result.Data;
			if (string.IsNullOrEmpty(user.AdamUserId))
			{
				_logger.LogError("Failed to deserialize response from {url}", currentUserUriBuilder.Uri);
				return (false, null);
			}

			return (true, user);
		}
	}
}
