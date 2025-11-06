# Receipt Reader

A .NET MAUI application integrated with .NET Aspire that uses Azure OpenAI's vision capabilities to extract information from receipt photos.

## Features

- Upload receipt photos from your device
- Automatic OCR extraction using Azure OpenAI GPT-4o with vision
- Displays merchant name, transaction date, total amount, and all line items
- Clean, responsive UI for mobile and desktop platforms
- .NET Aspire integration for orchestration and Azure OpenAI resource management

## Architecture

This solution follows .NET Aspire best practices:

- **ReceiptReader.AppHost**: Aspire AppHost that orchestrates the application and references Azure OpenAI connection
- **ReceiptReader**: MAUI client application that consumes the OpenAI service
- Uses connection string reference to existing Azure OpenAI resource
- Connection strings managed through Aspire's configuration system

## Prerequisites

- .NET 10 SDK (or compatible version)
- Azure OpenAI account with a vision-capable deployment (e.g., gpt-4o)
- Android, iOS, macOS, or Windows device for running the app
- .NET Aspire workload installed

## Configuration

### Important: MAUI Apps and Aspire Configuration

MAUI apps don't automatically receive environment variables from Aspire like web applications do. You need to configure the connection string in **both locations**:

### 1. Configure AppHost (for Aspire orchestration)

Edit `ReceiptReader.AppHost/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://YOUR-RESOURCE-NAME.openai.azure.com/;Key=YOUR-API-KEY;"
  }
}
```

### 2. Configure MAUI App (for local development)

Edit `ReceiptReader/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "openai": "Endpoint=https://YOUR-RESOURCE-NAME.openai.azure.com/;Key=YOUR-API-KEY;"
  },
  "AI": {
    "DeploymentName": "gpt-4o"
  }
}
```

**Connection String Format:**
- **Endpoint**: Your Azure OpenAI resource endpoint URL (e.g., `https://myresource.openai.azure.com/`)
- **Key**: Your Azure OpenAI API key
- **DeploymentName**: The name of your deployed model (default: "gpt-4o")

### Alternative: Using User Secrets (Recommended)

To avoid committing credentials to source control:

```bash
# For AppHost
cd ReceiptReader.AppHost
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://...;Key=...;"

# For MAUI app
cd ../ReceiptReader
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:openai" "Endpoint=https://...;Key=...;"
```

### 2. Build and Run

#### Using Visual Studio

1. Open `ReceiptReader.sln` in Visual Studio
2. Select your target platform (Android, iOS, Windows, etc.)
3. Press F5 to build and run

#### Using .NET CLI

For Android:
```bash
cd ReceiptReader
dotnet build -f net10.0-android
```

For Windows:
```bash
cd ReceiptReader
dotnet build -f net10.0-windows10.0.19041.0
```

## Project Structure

### ReceiptReader.AppHost
Aspire AppHost project that orchestrates the solution:
- References existing Azure OpenAI connection with `AddConnectionString("openai")`
- Passes connection to MAUI project with `WithReference(openai)`
- No automatic provisioning - uses your existing Azure resources

### ReceiptReader (MAUI App)
Main MAUI application:
- `Models/Receipt.cs`: Data models for receipt and line items
- `Services/ReceiptService.cs`: Service for processing receipt images with OpenAI
- `MainPage.xaml`: UI for uploading and displaying receipts
- `MauiProgram.cs`: Configures Azure OpenAI client from connection string
- `appsettings.json`: Configuration file with connection strings

### ReceiptReader.ServiceDefaults
Shared Aspire service defaults (not directly referenced by MAUI to avoid ASP.NET Core dependencies)

## How It Works

1. **Aspire AppHost**: Manages Azure OpenAI resource configuration and deployment
2. **MAUI App Startup**: Reads connection string from appsettings.json and creates AzureOpenAIClient
3. **User Interaction**: User selects a receipt photo from their device
4. **Image Display**: The selected image is displayed in the app
5. **AI Processing**: Image bytes are sent to Azure OpenAI's vision model (gpt-4o)
6. **Data Extraction**: AI extracts structured data (merchant, date, total, line items)
7. **Display Results**: Parsed receipt data is displayed in a formatted view

## Aspire Best Practices Implemented

This project follows the latest .NET Aspire best practices:

1. **Connection String Format**: Uses Aspire's standard connection string format (`Endpoint=...;Key=...;`)
2. **AddConnectionString Pattern**: AppHost uses `AddConnectionString()` to reference existing resources
3. **Resource References**: MAUI project references the OpenAI resource via `WithReference()`
4. **Separation of Concerns**: AppHost manages configuration orchestration, MAUI app focuses on UI/UX
5. **Configuration Management**: Centralized configuration through connection strings
6. **No Auto-Provisioning**: References existing Azure resources without automatic infrastructure provisioning

## Alternative: Auto-Provisioning Azure Resources

If you want Aspire to automatically provision Azure OpenAI resources (requires Azure CLI and authentication):

```csharp
// In AppHost.cs
var openai = builder.AddAzureOpenAI("openai");

var deployment = openai.AddDeployment(
    name: "gpt-4o",
    modelName: "gpt-4o",
    modelVersion: "2024-08-06"
);
```

**Note**: Auto-provisioning requires:
- Azure CLI installed and authenticated
- Proper Azure subscription permissions
- Valid Bicep tooling
- This will create real Azure resources and may incur costs

## Security Best Practices

**Important**: The `appsettings.json` file contains sensitive credentials.

### For Development:
- Keep `appsettings.json` with placeholder values in source control
- Use local configuration or user secrets for actual credentials
- Add `appsettings.json` with real values to `.gitignore`

### For Production (Recommended):
Use Aspire parameters with Azure Key Vault:

```csharp
var openAIKey = builder.AddParameter("OpenAIKey", secret: true);
var connectionString = builder.AddConnectionString(
    "openai",
    ReferenceExpression.Create($"Endpoint=https://my-resource.openai.azure.com/;Key={openAIKey}")
);
```

Benefits:
- Secrets stored securely in Azure Key Vault
- No credentials in source control or configuration files
- Automatic secret rotation support
- Environment-specific configuration

## Troubleshooting

### "OpenAI connection string must be configured"

Make sure you've configured the connection string in **both**:
1. `ReceiptReader.AppHost/appsettings.Development.json`
2. `ReceiptReader/appsettings.json`

Format:
```
Endpoint=https://YOUR-RESOURCE.openai.azure.com/;Key=YOUR-KEY;
```

### MAUI app not picking up Aspire configuration

Unlike ASP.NET Core apps, MAUI apps don't automatically receive environment variables from Aspire's orchestration. The MAUI app reads configuration from:
1. Embedded `appsettings.json`
2. Environment variables (fallback)
3. User secrets (if configured)

This is why you need to configure the connection string directly in the MAUI app's `appsettings.json` for local development.

### Build errors related to AspNetCore.App

The MAUI app should not reference `ReceiptReader.ServiceDefaults` directly as it contains ASP.NET Core dependencies. The current configuration properly isolates these concerns.

### Image upload not working

Ensure your app has the necessary permissions to access photos on your device. Check platform-specific permission settings in the project properties.

### Aspire Dashboard

To view telemetry and monitor the application, run the AppHost project and navigate to the Aspire dashboard (typically http://localhost:15888).

## License

MIT
