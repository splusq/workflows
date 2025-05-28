using System.ClientModel.Primitives;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;


public class Agent
{
    private readonly AssistantClient client;

    public Agent(string endpoint, string audience, string apiVersion)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var _endpoint))
        {
            throw new ArgumentException("The AZURE_AI_AGENTS_ENDPOINT environment variable must be a valid URI.");
        }

        this.client = new AzureOpenAIClient(_endpoint, new DefaultAzureCredential(), new AzureOpenAIClientOptions().AddPolicy(_endpoint, audience, apiVersion)).GetAssistantClient();
    }

    public static implicit operator AssistantClient(Agent agent) => agent.client;
}

public static class Extension
{
    public static AzureOpenAIClientOptions AddPolicy(this AzureOpenAIClientOptions options, Uri endpoint, string audience, string apiVersion)
    {
        options.Audience = audience;
        options.AddPolicy(new PipelineRoutingPolicy(endpoint, apiVersion), PipelinePosition.PerCall);

        return options;
    }
}