using Azure.AI.Agents.Persistent;
using Azure.Core;
using Azure.Core.Pipeline;

public class RoutingPolicy : HttpPipelinePolicy
{
    public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        Process(message);

        ProcessNext(message, pipeline);
    }

    public override ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        Process(message);

        return ProcessNextAsync(message, pipeline);
    }

    public void Process(HttpMessage message)
    {
        if (message.Request.Uri is null)
        {
            throw new ArgumentException(nameof(message.Request.Uri));
        }

        message.Request.Uri.Reset(message.Request.Uri.ToUri().Reroute(apiVersion: Environment.GetEnvironmentVariable("AZURE_AI_API_VERSION")!, isWorkflow: message.Request.Content!.IsWorkflow()));
    }
}
