using Azure.AI.Agents.Persistent;
using Azure.Core;

public static class Extension
{
    public static PersistentAgentsAdministrationClientOptions WithPolicy(this PersistentAgentsAdministrationClientOptions options, string endpoint, string apiVersion)
    {
        if (Uri.TryCreate(endpoint, UriKind.Absolute, out var _endpoint) is false)
        {
            throw new ArgumentException("Invalid endpoint URI.", nameof(endpoint));
        }

        options.AddPolicy(new HttpPipelineRoutingPolicy(_endpoint, apiVersion), HttpPipelinePosition.PerCall);

        return options;
    }
}