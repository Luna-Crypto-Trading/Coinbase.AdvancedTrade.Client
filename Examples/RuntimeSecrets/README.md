# Runtime Secrets Injection Example

This example demonstrates various ways to inject API credentials at runtime, rather than storing them in configuration files. This is crucial for production applications where secrets should never be hardcoded.

## Security Best Practices

🔐 **Never store API secrets in:**
- Configuration files committed to version control
- Environment variables in production (unless properly secured)
- Hardcoded strings in your application

✅ **Instead use:**
- Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault
- Kubernetes secrets with proper RBAC
- Environment variables only in secure, non-logged environments
- User input for development/testing scenarios

## Running the Example

```bash
cd Examples/RuntimeSecrets
dotnet run
```

## Methods Demonstrated

### 1. Environment Variables
Set your credentials as environment variables:

**Linux/macOS:**
```bash
export COINBASE_API_KEY="your-api-key"
export COINBASE_API_SECRET="your-api-secret"
dotnet run
```

**Windows:**
```cmd
set COINBASE_API_KEY=your-api-key
set COINBASE_API_SECRET=your-api-secret
dotnet run
```

### 2. Interactive User Input
The application prompts you to enter credentials at runtime with masked password input.

### 3. External Service Simulation
Demonstrates how to retrieve credentials from external key management services.

## Key Features Demonstrated

- **Credential validation**: Verify API keys before use
- **Multiple injection methods**: Environment variables, user input, external services  
- **Runtime configuration**: Update client settings after startup
- **Security practices**: Password masking, credential validation
- **Error handling**: Graceful handling of invalid credentials

## Production Implementation Examples

### Azure Key Vault
```csharp
var credential = new DefaultAzureCredential();
var client = new SecretClient(new Uri("https://vault.vault.azure.net/"), credential);

var apiKeySecret = await client.GetSecretAsync("coinbase-api-key");
var apiSecretSecret = await client.GetSecretAsync("coinbase-api-secret");

var settings = new CoinbaseSettings
{
    ApiKey = apiKeySecret.Value.Value,
    ApiSecret = apiSecretSecret.Value.Value,
    Sandbox = false
};
```

### AWS Secrets Manager
```csharp
var client = new AmazonSecretsManagerClient();
var request = new GetSecretValueRequest
{
    SecretId = "prod/coinbase/credentials"
};

var response = await client.GetSecretValueAsync(request);
var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);

var settings = new CoinbaseSettings
{
    ApiKey = secrets["ApiKey"],
    ApiSecret = secrets["ApiSecret"],
    Sandbox = false
};
```

### Kubernetes Secrets
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: coinbase-credentials
type: Opaque
data:
  api-key: <base64-encoded-api-key>
  api-secret: <base64-encoded-api-secret>
```

```csharp
// Read from mounted secret files
var apiKey = await File.ReadAllTextAsync("/etc/secrets/api-key");
var apiSecret = await File.ReadAllTextAsync("/etc/secrets/api-secret");
```

## Code Breakdown

### Runtime Settings Update
```csharp
private IServiceScope CreateScopeWithSettings(CoinbaseSettings settings)
{
    var services = new ServiceCollection();
    services.Configure<CoinbaseSettings>(_ => 
    {
        _.ApiKey = settings.ApiKey;
        _.ApiSecret = settings.ApiSecret; 
        _.Sandbox = settings.Sandbox;
    });
    services.AddCoinbaseAdvancedTradeClient();
    
    var provider = services.BuildServiceProvider();
    return provider.CreateScope();
}
```

### Credential Validation
```csharp
var validationResult = await _validator.ValidateCredentialsAsync(apiKey, apiSecret);
if (!validationResult.IsValid)
{
    Console.WriteLine($"❌ Credential validation failed: {validationResult.ErrorMessage}");
    return;
}
```

This approach ensures your API credentials are handled securely and can be updated without application restarts.