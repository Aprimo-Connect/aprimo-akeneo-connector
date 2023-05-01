using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace API.Tokens
{
	public class FileSystemTokenStorage : ITokenStorage
	{
		private readonly FileSystemTokenStorageOptions _options;
		private readonly IDataProtector _dataProtector;

		public FileSystemTokenStorage(FileSystemTokenStorageOptions options, IDataProtectionProvider dataProtectionProvider)
		{
			_options = options;
			_dataProtector = dataProtectionProvider.CreateProtector("FileSystemTokenStorage");
		}

		public Task<string?> GetTokenAsync(string id)
		{
			var tokens = LoadTokens();
			if (tokens.TryGetValue(id, out var token))
			{
				return Task.FromResult<string?>(token);
			}

			return Task.FromResult<string?>(null);
		}

		public Task SetTokenAsync(string id, string token)
		{
			var tokens = LoadTokens();
			tokens[id] = token;
			SaveTokens(tokens);

			return Task.CompletedTask;
		}

		private Dictionary<string, string> LoadTokens()
		{
			var tokens = new Dictionary<string, string>();
			if (!File.Exists(_options.Path))
			{
				return tokens;
			}

			var tokenFile = Encoding.UTF8.GetString(File.ReadAllBytes(_options.Path));
			var parsedTokens = tokenFile
				.Split(Environment.NewLine)
				.Select(s => s.Split(':', 2))
				.Where(parts => parts.Length == 2)
				.Select(parts =>
				{
					try
					{
						var unprotectedToken = _dataProtector.Unprotect(parts[1]);
						return (Success: true, (Id: parts[0], Token: unprotectedToken));
					}
					catch (CryptographicException) { }

					return (Success: false, (Id: parts[0], Token: string.Empty));
				})
				.Where(result => result.Success)
				.Select(result => result.Item2);

			foreach (var parsedToken in parsedTokens)
			{
				tokens[parsedToken.Id] = parsedToken.Token;
			}

			return tokens;
		}

		private void SaveTokens(Dictionary<string, string> tokens)
		{
			var output = tokens.Select(kvp =>
			{
				return $"{kvp.Key}:{_dataProtector.Protect(kvp.Value)}";
			});

			using (var writer = File.OpenWrite(_options.Path))
			{
				writer.Write(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, output)));
			}
		}
	}

	public class FileSystemTokenStorageOptions
	{
		public string Path { get; set; } = string.Empty;

		public FileSystemTokenStorageOptions() { }

		public FileSystemTokenStorageOptions(string path)
		{
			Path = path;
		}
	}
}
