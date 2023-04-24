using API.Akeneo;
using API.Aprimo;
using API.Configuration;
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
			builder.Services.AddHttpClient<IAkeneoService, AkeneoService>();
			builder.Services.AddHttpClient<IAprimoTokenService, AprimoTokenService>();

			builder.Services.AddScoped<IAprimoUserRepository, AprimoUserRepository>();

			builder.Services.AddFileSystemTokenStorage(options =>
			{
				options.Path = Path.Combine(AppContext.BaseDirectory, "token");
			});

			builder.Services.AddControllers();

			builder.Services.AddAuthentication().AddScheme<AprimoRuleAuthenticationHandlerOptions, AprimoRuleAuthenticationHandler>("AprimoRuleAuth", null);
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			});

			builder.Services.AddSettings(builder.Configuration);

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}