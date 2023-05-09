using System.Text.Json;

namespace API
{
	public static class HttpClientExtensions
	{
		public static Task<(bool Success, WrappedResponse<T>? Result)> SendRequestAsyncWithResult<T>(this HttpClient client, HttpRequestMessage request)
		{
			return client.SendRequestAsyncWithResult<T>(request, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
			});
		}

		public static async Task<(bool Success, WrappedResponse<T>? Result)> SendRequestAsyncWithResult<T>(this HttpClient client, HttpRequestMessage request, JsonSerializerOptions serializerOptions)
		{
			var response = await client.SendAsync(request);
			var wrappedResponse = new WrappedResponse<T> { Response = response };
			var responseContent = await response.Content.ReadAsStringAsync();
			if (!response.IsSuccessStatusCode)
			{
				//_logger.LogError("Request to {url} failed with status code {statusCode}", request.RequestUri, response.StatusCode);
				return (false, wrappedResponse);
			}

			var result = JsonSerializer.Deserialize<T>(responseContent, serializerOptions);
			if (result == null)
			{
				//_logger.LogError("Failed to deserialize response from {url}: {body}", request.RequestUri, responseContent);
				return (false, wrappedResponse);
			}

			wrappedResponse.Data = result;
			return (true, wrappedResponse);
		}

		public class WrappedResponse<T>
		{
			public T? Data { get; set; }

			public HttpResponseMessage? Response { get; set; }
		}
	}
}
