using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;
using System.ClientModel.Primitives;


public class Agent
{
    private readonly AssistantClient client;

    public Agent(string endpoint, string apiVersion)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("The AZURE_AI_AGENTS_ENDPOINT environment variable must be a valid URI.");
        }

        this.client = new AzureOpenAIClient(uri, new DefaultAzureCredential(), new AzureOpenAIClientOptions().AddPolicy(apiVersion)).GetAssistantClient();
    }

    public static implicit operator AssistantClient(Agent agent) => agent.client;
}

public static class Extension
{
    public static AzureOpenAIClientOptions AddPolicy(this AzureOpenAIClientOptions options, string apiVersion)
    {
        options.AddPolicy(new PipelineRoutingPolicy(apiVersion), PipelinePosition.PerCall);

        return options;
    }
}