using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI.Assistants;

public class Agent
{
    private readonly AssistantClient client;

    public Agent()
    {
        if (!Uri.TryCreate(Environment.GetEnvironmentVariable("AZURE_AI_AGENTS_ENDPOINT"), UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("The AZURE_AI_AGENTS_ENDPOINT environment variable must be a valid URI.");
        }

        this.client = new AzureOpenAIClient(uri, new DefaultAzureCredential(), ClientOptions.Default).GetAssistantClient();
    }

    public static implicit operator AssistantClient(Agent agent) => agent.client;
}
