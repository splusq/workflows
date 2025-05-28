using Azure.AI.Agents.Persistent;
using Azure.Core;

public static class Extension
{
    public static PersistentAgentsAdministrationClientOptions AddPolicy(this PersistentAgentsAdministrationClientOptions options, string endpoint, string apiVersion)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var _endpoint))
        {
            throw new ArgumentException("The endpoint must be an absolute URI.", nameof(endpoint));
        }

        options.AddPolicy(new HttpPipelineRoutingPolicy(_endpoint, apiVersion), HttpPipelinePosition.PerCall);

        return options;
    }
}