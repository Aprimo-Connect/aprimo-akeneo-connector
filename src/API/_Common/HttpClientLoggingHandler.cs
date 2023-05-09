﻿namespace API._Common
{
	public class HttpClientLoggingHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Console.WriteLine("Request:");
			Console.WriteLine(request.ToString());
			if (request.Content != null)
			{
				Console.WriteLine(await request.Content.ReadAsStringAsync());
			}
			Console.WriteLine();

			HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

			Console.WriteLine("Response:");
			Console.WriteLine(response.ToString());
			if (response.Content != null)
			{
				Console.WriteLine(await response.Content.ReadAsStringAsync());
			}
			Console.WriteLine();

			return response;
		}
	}
}
