using Azure.AI.Agents.Persistent;
using Azure.Core;

public static class Extension
{
    public static PersistentAgentsAdministrationClientOptions AddPolicy(this PersistentAgentsAdministrationClientOptions options, string apiVersion)
    {
        options.AddPolicy(new HttpPipelineRoutingPolicy(apiVersion), HttpPipelinePosition.PerCall);

        return options;
    }
}