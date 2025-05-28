using System.ClientModel.Primitives;
using Azure.Core;
using Azure.Core.Pipeline;

public class HttpPipelineRoutingPolicy : HttpPipelinePolicy
{
    private readonly Uri endpoint;
    private readonly string apiVersion;

    public HttpPipelineRoutingPolicy(Uri endpoint, string apiVersion)
    {
        this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
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
        else if (message.Request.Uri.ToUri().IsLoopback)
        {
            message.Request.Uri.Reset(new Uri(String.Format($"{this.endpoint.ToString().TrimEnd('/')}/{message.Request.Uri.ToUri().AbsolutePath.ToString().TrimStart('/')}?api-version={this.apiVersion}")));
        }


        message.Request.Uri.Reset(message.Request.Uri.ToUri().Reroute(apiVersion: apiVersion, isWorkflow: message.Request.Content!.IsWorkflow()));
    }
}

public class PipelineRoutingPolicy : PipelinePolicy
{
    private readonly Uri endpoint;
    private readonly string apiVersion;

    public PipelineRoutingPolicy(Uri endpoint, string apiVersion)
    {
        this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
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

            throw new ArgumentNullException(nameof(request.Uri));
        }
        else if (request.Uri.IsLoopback)
        {
            request.Uri = new Uri(String.Format($"{this.endpoint.ToString().TrimEnd('/')}/{request.Uri.AbsolutePath.ToString().TrimStart('/')}?api-version={this.apiVersion}"));
        }

        request.Uri = request.Uri.Reroute(apiVersion: apiVersion, isWorkflow: request.Content!.IsWorkflow());
    }
}