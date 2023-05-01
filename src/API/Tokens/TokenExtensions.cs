namespace API.Tokens
{
	public static class IServiceCollectionExtensions
	{
		public static IServiceCollection AddTokenStorage<V>(this IServiceCollection services) where V : class, ITokenStorage
		{
			services.AddSingleton<ITokenStorage, V>();
			return services;
		}

		public static IServiceCollection AddFileSystemTokenStorage(this IServiceCollection services, Action<FileSystemTokenStorageOptions> optionsBuilder)
		{
			services.AddSingleton((_) =>
			{
				var options = new FileSystemTokenStorageOptions();
				optionsBuilder(options);
				return options;
			});
			services.AddTokenStorage<FileSystemTokenStorage>();
			return services;
		}
	}
}
