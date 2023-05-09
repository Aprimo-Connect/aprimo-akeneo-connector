using API.Akeneo;
using API.Aprimo;
using API.Configuration;
using API.Integration;
using API.Tokens;
using System.Reflection;

namespace API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddDataProtection();
			builder.Services.AddControllers();
			builder.Services.AddAkeneo(builder.Configuration);
			builder.Services.AddAprimo(builder.Configuration);
			builder.Services.AddIntegration();
			builder.Services.AddFileSystemTokenStorage((storageOptions) =>
			{
				storageOptions.Path = Path.Combine(AppContext.BaseDirectory, "tokens");
			});

			builder.Services.AddSettings(builder.Configuration);

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.UseAkeneo();
			app.UseAprimo();

			app.MapControllers();

			app.Run();
		}
	}
}