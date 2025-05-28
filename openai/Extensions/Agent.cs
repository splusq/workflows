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
            throw new ArgumentException("The endpoint must be an absolute URI.", nameof(endpoint));
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