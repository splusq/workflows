using Azure.Core;
using Azure.Core.Pipeline;
using System.ClientModel.Primitives;

public class HttpPipelineRoutingPolicy : HttpPipelinePolicy
{
    private readonly string apiVersion;

    public HttpPipelineRoutingPolicy(string apiVersion)
    {
        this.apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
    }

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

        message.Request.Uri.Reset(message.Request.Uri.ToUri().Reroute(apiVersion: apiVersion, isWorkflow: message.Request.Content!.IsWorkflow()));
    }
}

public class PipelineRoutingPolicy : PipelinePolicy
{
    private readonly string apiVersion;

    public PipelineRoutingPolicy(string apiVersion)
    {
        this.apiVersion = apiVersion ?? throw new ArgumentNullException(nameof(apiVersion));
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

        request.Uri = request.Uri.Reroute(apiVersion: apiVersion, isWorkflow: request.Content!.IsWorkflow());
    }
}