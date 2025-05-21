using Azure.AI.OpenAI;
using System.ClientModel.Primitives;

/// <summary>
/// A specialization of <see cref="AzureOpenAIClientOptions"/> intended to allow the customization of an
/// <see cref="AzureOpenAIClient"/> to use the Azure AI Agents service.
/// </summary>
/// <remarks>
/// As the Azure AI Agents service is distinct from the Azure OpenAI Assistants API, compatibility is subject to change
/// across API versions.
/// </remarks>
public class ClientOptions : AzureOpenAIClientOptions
{
    /// <summary>
    /// The <c>api-version</c> query string parameter value to use when connecting to Azure AI Agents.
    /// </summary>
    public required string ApiVersion { get; set; }

    public static ClientOptions Default {
        get
        {
            return new ClientOptions()
            {
                ApiVersion = Environment.GetEnvironmentVariable("AZURE_AI_API_VERSION")!,
                Audience = Environment.GetEnvironmentVariable("AZURE_AI_AUDIENCE")!,
            };
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="AzureAIAgentClientOptions"/> that will customize an
    /// <see cref="AzureOpenAIClient"/> for use with the Azure AI Agents service.
    /// </summary>
    /// <remarks>
    /// A <see cref="PipelinePolicy"/> will automatically be applied that performs request customizations. These
    /// customizations include:
    /// <para>
    /// <list type="bullet">
    /// <item>The <c>/openai</c> request URI path infix is removed</item>
    /// <item>The <c>include[]</c> query string parameter is automatically emplaced if not already present</item>
    /// <item>The <c>api-version</c> query string parameter is updated to the provided custom value</item>
    /// <item>Any specified additional headers are added to the request</item>
    /// </list>
    /// </para>
    /// </remarks>
    public ClientOptions() : base()
    {
        AddPolicy(new RoutingPolicy(this), PipelinePosition.PerCall);
    }

    internal class RoutingPolicy : PipelinePolicy
    {
        public ClientOptions ParentOptions { get; }

        public RoutingPolicy(ClientOptions parentOptions)
        {
            ParentOptions = parentOptions;
        }

        public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ProcessRequest(message.Request);
            ProcessNext(message, pipeline, currentIndex);
        }

        public override async ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
        {
            ProcessRequest(message.Request);
            await ProcessNextAsync(message, pipeline, currentIndex);
        }

        private void ProcessRequest(PipelineRequest request)
        {
            if (request.Uri is null)
            {
                throw new ArgumentException(nameof(request.Uri));
            }

            request.Uri = request.Uri.Reroute(apiVersion: ParentOptions.ApiVersion, isWorkflow: request.Content!.IsWorkflow());
        }
    }
}