using Azure.Identity;
using System.Text.Json;
using System.Text;
using Microsoft.SemanticKernel;
using System.Text.RegularExpressions;
using Azure.Core;

public static class Ext
{
    public static Uri WorkflowEndpoint
    {
        get
        {
            if (!Uri.TryCreate(Environment.GetEnvironmentVariable("AZURE_AI_AGENTS_ENDPOINT")?.Replace("/agents/v1.0", "/workflows/v1.0"), UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("The AZURE_AI_AGENTS_ENDPOINT environment variable must be a valid URI.");
            }

            return uri;
        }
    }

    private static HttpClient _client;

    static Ext()
    {
        _client = new HttpClient() { BaseAddress = WorkflowEndpoint };
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {(new DefaultAzureCredential().GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { Environment.GetEnvironmentVariable("AZURE_AI_AUDIENCE")! }))).Result.Token}");
    }

    public static async Task<Workflow> PublishWorkflowAsync<T>(this FoundryProcessBuilder<T> process) where T : class, new()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"agents?api-version={Environment.GetEnvironmentVariable("AZURE_AI_API_VERSION")}")
        {
            Content = new StringContent(await process.ToJsonAsync(), Encoding.UTF8, "application/json")
        };

        // Console.WriteLine($"Posting workflow to {_client.BaseAddress}{request.RequestUri}");
        // Console.WriteLine(definition);

        var response = await _client.SendAsync(request).ConfigureAwait(false);
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception($"Error publishing workflow: {errorContent}", ex);
        }

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false) ?? string.Empty;

        using (var doc = JsonDocument.Parse(json))
        {
            var workflowId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

            Console.WriteLine($"Creating workflow {workflowId}...");

            return new Workflow(workflowId);
        }
    }

    public static async Task DeleteWorkflowAsync(this Workflow workflow)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"agents/{workflow.Id}");
        var response = await _client.SendAsync(request).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public static Uri Reroute(this Uri uri, string apiVersion, bool isWorkflow)
    {
        UriBuilder uriBuilder = new(uri);

        // Check if URI contains "run" and body contains assistant_id starting with "wf_"
        if (uri.ToString().Contains("runs", StringComparison.OrdinalIgnoreCase))
        {
            if (isWorkflow)
            {
                uriBuilder.Path = Regex.Replace(uriBuilder.Path, "/agents/v1.0", "/workflows/v1.0");
            }
        }

        // Remove the "/openai" request URI infix, if present
        uriBuilder.Path = Regex.Replace(uriBuilder.Path, "/openai", string.Empty);

        // Substitute the Azure AI Agents api-version where the default AOAI one is
        uriBuilder.Query = Regex.Replace(uriBuilder.Query, "api-version=[^&]*", $"api-version={apiVersion}");

        // Ensure file search citation result content is always requested on run steps
        if (!uriBuilder.Query.Contains("include[]"))
        {
            uriBuilder.Query += "&include[]=step_details.tool_calls[*].file_search.results[*].content";
        }

        return uriBuilder.Uri;
    }

    private static bool StreamContainsWorkflowPattern(Stream stream)
    {
        var pattern = Encoding.UTF8.GetBytes("\"assistant_id\":\"wf_");
        stream.Position = 0;
        int matchIndex = 0;
        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            if (b == pattern[matchIndex])
            {
                matchIndex++;
                if (matchIndex == pattern.Length)
                    return true;
            }
            else
            {
                matchIndex = (b == pattern[0]) ? 1 : 0;
            }
        }

        return false;
    }

    public static bool IsWorkflow(this RequestContent content)
    {
        try
        {
            using var stream = new MemoryStream();
            content?.WriteTo(stream, default);
            return StreamContainsWorkflowPattern(stream);
        }
        catch
        {
            // ignore
        }
        return false;
    }

    public static bool IsWorkflow(this System.ClientModel.BinaryContent content)
    {
        try
        {
            using var stream = new MemoryStream();
            content?.WriteTo(stream, default);
            return StreamContainsWorkflowPattern(stream);
        }
        catch
        {
            // ignore
        }
        return false;
    }
}