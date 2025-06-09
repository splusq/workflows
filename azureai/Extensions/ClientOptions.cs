using Azure.AI.Projects;
using Azure.Core;

public static class Extension
{
    public static AIProjectClientOptions WithPolicy(this AIProjectClientOptions options, Uri endpoint, string apiVersion)
    {
        options.AddPolicy(new HttpPipelineRoutingPolicy(endpoint, apiVersion), HttpPipelinePosition.PerCall);

        return options;
    }
}