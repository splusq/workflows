
## Windows

Clone a2a-net repo
```bash
git clone https://github.com/neuroglia-io/a2a-net
cd a2a-net
```

Install .NET SDK 9.0 (if needed)

Download and install the .NET SDK 9.0 from the official Microsoft website:

- [Download .NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

Alternatively, using winget (Windows Package Manager):

```powershell
winget install Microsoft.DotNet.SDK.9
```

Ensure Azure CLI is installed. If not, install it from [Azure CLI](https://aka.ms/installazurecliwindows).  
Log in to Azure:

```powershell
az login
```

Get the access token:
```powershell
$token = az account get-access-token --scope "https://ai.azure.com/.default" --query accessToken -o tsv
```

Interact with an existing foundry agent using dotnet a2a client
```powershell
dotnet run --project .\samples\semantic-kernel\a2a-net.Samples.SemanticKernel.Client\a2a-net.Samples.SemanticKernel.Client.csproj `
  --server "https://eastus.api.azureml.ms/workflows/a2a/v1.0/subscriptions/921496dc-987f-410f-bd57-426eb2611356/resourceGroups/ai-agents-karthik-eu/providers/Microsoft.MachineLearningServices/workspaces/project-demo-eu-fw7g/agents/asst_8vfrJwY26XYXzNfCzhJu2IZA?api-version=2024-12-01-preview" `
  --auth "Bearer=$token" `
  --streaming
```

## Linux

CLone a2a-net repo
```bash
git clone https://github.com/neuroglia-io/a2a-net
cd a2a-net
```

Install .NET SDK 9.0 (if needed)
```bash
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
```

```bash
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-9.0
```

Install Azure CLI (if needed)
```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

Authenticate with Azure CLI
```bash
az login
```

Get the access token:
```bash
token=$(az account get-access-token --scope "https://ai.azure.com/.default" --query accessToken -o tsv)
```

Interact with an existing foundry agent using dotnet a2a client
```bash
dotnet run --project ./samples/semantic-kernel/a2a-net.Samples.SemanticKernel.Client/a2a-net.Samples.SemanticKernel.Client.csproj --server https://eastus.api.azureml.ms/workflows/a2a/v1.0/subscriptions/921496dc-987f-410f-bd57-426eb2611356/resourceGroups/ai-agents-karthik-eu/providers/Microsoft.MachineLearningServices/workspaces/project-demo-eu-fw7g/agents/asst_8vfrJwY26XYXzNfCzhJu2IZA?api-version=2024-12-01-preview --auth "Bearer=$token" --streaming
```

## Prompts
- What are the current 30-year fixed mortgage rates in California?
- What is the mortgage rate trend for the past 3 months?
- Are there any new homebuyer assistance programs in Texas? 
- Has the FHA loan limit changed for 2024?
- Is it a good time to refinance my mortgage?
- What is the forecast for housing prices in 2024? 
- What are the best cities for first-time homebuyers in the US?
- Are mortgage rates expected to drop in Q3 2024?
- What are current real estate trends in Seattle?
- What are the new tax benefits for homeowners in 2024?