using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

namespace API.Akeneo
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

		public Task<string?> GetTokenAsync()
		{
			if (!File.Exists(_options.Path))
			{
				return Task.FromResult<string?>(null);
			}

			var protectedToken = Encoding.UTF8.GetString(File.ReadAllBytes(_options.Path));

			try
			{
				return Task.FromResult<string?>(_dataProtector.Unprotect(protectedToken));
			}
			catch (CryptographicException) { }

			return Task.FromResult<string?>(null);
		}

		public Task SetTokenAsync(string token)
		{
			using (var writer = File.OpenWrite(_options.Path))
			{
				writer.Write(Encoding.UTF8.GetBytes(_dataProtector.Protect(token)));
			}

			return Task.CompletedTask;
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

	public static class IServiceCollection_Extensions
	{
		public static IServiceCollection AddFileSystemTokenStorage(this IServiceCollection services, Action<FileSystemTokenStorageOptions> optionsBuilder)
		{
			services.AddSingleton((_) =>
			{
				var options = new FileSystemTokenStorageOptions();
				optionsBuilder(options);
				return options;
			});
			services.AddSingleton<ITokenStorage, FileSystemTokenStorage>();
			return services;
		}
	}
}
