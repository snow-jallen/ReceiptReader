using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ReceiptReader.Services;
using Azure.AI.OpenAI;
using Azure;
using System.Reflection;

namespace ReceiptReader;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Add configuration from embedded appsettings.json
		var assembly = Assembly.GetExecutingAssembly();
		using var stream = assembly.GetManifestResourceStream("ReceiptReader.appsettings.json");
		if (stream != null)
		{
			var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
				.AddJsonStream(stream)
				.AddEnvironmentVariables() // Add environment variables (from Aspire)
				.Build();

			foreach (var kvp in config.AsEnumerable())
			{
				if (kvp.Value != null)
				{
					builder.Configuration[kvp.Key] = kvp.Value;
				}
			}
		}

		// Add Azure OpenAI client using connection string format
		builder.Services.AddSingleton<AzureOpenAIClient>(sp =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();
			var connectionString = configuration.GetConnectionString("openai");

			if (string.IsNullOrEmpty(connectionString))
			{
				throw new InvalidOperationException("OpenAI connection string must be configured in appsettings.json");
			}

			// Parse connection string (format: "Endpoint=https://...;Key=...;")
			var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
			var endpoint = parts.FirstOrDefault(p => p.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))
				?["Endpoint=".Length..];
			var key = parts.FirstOrDefault(p => p.StartsWith("Key=", StringComparison.OrdinalIgnoreCase))
				?["Key=".Length..];

			if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
			{
				throw new InvalidOperationException("Connection string must include both Endpoint and Key");
			}

			return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
		});

		// Register services
		builder.Services.AddSingleton<ReceiptService>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
