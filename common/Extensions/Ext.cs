using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.SemanticKernel;
using System;
using System.ClientModel.Primitives;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class Ext
{
    public static async Task<Workflow> PublishWorkflowAsync(this ClientPipeline pipeline, WorkflowBuilder workflowDefinition, string? id = null)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        var payload = workflowDefinition.BuildJson().ToString();
        message.Request.Method = "POST";
        message.Request.Uri = new Uri("https://localhost/agents" + (id != null ? $"/{id}" : string.Empty));
        message.Request.Content = System.ClientModel.BinaryContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(payload)));
        message.Request.Headers.Add("Content-Type", "application/json");

        await pipeline.SendAsync(message).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);

            throw new Exception($"Error publishing workflow: {errorContent}");
        }

        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;

        using var doc = JsonDocument.Parse(responseJson);
        var workflowId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        Console.WriteLine($"Creating workflow {workflowId}...");

        return new Workflow(workflowId);
    }

    public static async Task<Workflow> PublishWorkflowAsync(this HttpPipeline pipeline, WorkflowBuilder workflowDefinition, string? id = null)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Post;
        message.Request.Uri.Reset(new Uri("https://localhost/agents" + (id != null ? $"/{id}" : string.Empty)));
        message.Request.Content = RequestContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(workflowDefinition.BuildJson().ToString())));
        message.Request.Headers.Add("Content-Type", "application/json");

        await pipeline.SendAsync(message, default).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);

            throw new Exception($"Error publishing workflow: {errorContent}");
        }

        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;

        using var doc = JsonDocument.Parse(responseJson);
        var workflowId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        Console.WriteLine($"Creating workflow {workflowId}...");

        return new Workflow(workflowId);
    }

    public static async Task<Workflow> PublishWorkflowAsync<T>(this ClientPipeline pipeline, FoundryProcessBuilder<T> process) where T : class, new()
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        var payload = await process.ToJsonAsync();
        message.Request.Method = "POST";
        message.Request.Uri = new Uri("https://localhost/agents");
        message.Request.Content = System.ClientModel.BinaryContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(payload)));
        message.Request.Headers.Add("Content-Type", "application/json");

        await pipeline.SendAsync(message).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);

            throw new Exception($"Error publishing workflow: {errorContent}");
        }

        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;

        using var doc = JsonDocument.Parse(responseJson);
        var workflowId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        Console.WriteLine($"Creating workflow {workflowId}...");

        return new Workflow(workflowId);
    }

    public static async Task<Workflow> PublishWorkflowAsync<T>(this HttpPipeline pipeline, FoundryProcessBuilder<T> process) where T : class, new()
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Post;
        message.Request.Uri.Reset(new Uri("https://localhost/agents"));
        message.Request.Content = RequestContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(await process.ToJsonAsync())));
        message.Request.Headers.Add("Content-Type", "application/json");

        await pipeline.SendAsync(message, default).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);

            throw new Exception($"Error publishing workflow: {errorContent}");
        }

        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;

        using var doc = JsonDocument.Parse(responseJson);
        var workflowId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

        Console.WriteLine($"Creating workflow {workflowId}...");

        return new Workflow(workflowId);
    }

    public static async Task DeleteWorkflowAsync(this ClientPipeline pipeline, Workflow workflow)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = "DELETE";
        message.Request.Uri = new Uri($"https://localhost/agents/{workflow.Id}");

        await pipeline.SendAsync(message).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            throw new Exception($"Failed to delete workflow: {message.Response?.Status} {message.Response?.ReasonPhrase}");
        }
    }

    public static async Task DeleteWorkflowAsync(this HttpPipeline pipeline, Workflow workflow)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Delete;
        message.Request.Uri.Reset(new Uri($"https://localhost/agents/{workflow.Id}"));

        await pipeline.SendAsync(message, default).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            throw new Exception($"Failed to delete workflow: {message.Response?.Status} {message.Response?.ReasonPhrase}");
        }
    }

    public static Uri Reroute(this Uri uri, string apiVersion, bool isWorkflow)
    {
        UriBuilder uriBuilder = new(uri);

        // Check if URI contains "run" and body contains assistant_id starting with "wf_"
        bool isRunOrAgentPath =
           uri.ToString().Contains("runs", StringComparison.OrdinalIgnoreCase) ||
           uri.AbsolutePath.EndsWith("/agents");

        bool isWorkflowInstance =
            uri.AbsolutePath.Contains("/wf_agent");

        bool shouldRewriteToWorkflow =
            (isRunOrAgentPath && isWorkflow) || isWorkflowInstance;

        if (shouldRewriteToWorkflow)
        {
            // 1RP
            if (uriBuilder.Host.EndsWith("services.ai.azure.com", StringComparison.OrdinalIgnoreCase))
            {
                var items = new ArrayList(uriBuilder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries));
                if (items.Count > 3)
                {
                    items.Insert(3, "workflows");
                }

                uriBuilder.Path = string.Join("/", items.ToArray());
            }
            else
            {
                // Non-1RP (Machine Learning RP)
                uriBuilder.Path = Regex.Replace(uriBuilder.Path, "/agents/v1.0", "/workflows/v1.0", RegexOptions.IgnoreCase);
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

    public static async Task<string> AsJsonAsync(this BinaryData data)
    {
        if (data == null || data.Length == 0)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(data.ToStream(), Encoding.UTF8);

        return await reader.ReadToEndAsync();
    }

    private static bool StreamContainsWorkflowPattern(Stream stream, params string[] bodies)
    {
        var patterns = bodies.Select(b => Encoding.UTF8.GetBytes(b)).ToArray();
        stream.Position = 0;
        int b;
        var matchIndexes = new int[patterns.Length];
        while ((b = stream.ReadByte()) != -1)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                if (b == patterns[i][matchIndexes[i]])
                {
                    matchIndexes[i]++;
                    if (matchIndexes[i] == patterns[i].Length)
                        return true;
                }
                else
                {
                    matchIndexes[i] = (b == patterns[i][0]) ? 1 : 0;
                }
            }
        }
        return false;
    }

    private static bool IsWorkflowInternal<T>(T content, Action<T, Stream> writeToStream)
    {
        try
        {
            using var stream = new MemoryStream();
            writeToStream(content, stream);
            return StreamContainsWorkflowPattern(stream, @"""assistant_id"":""wf_", @"""workflow_version");
        }
        catch
        {
            // ignore
        }
        return false;
    }

    public static bool IsWorkflow(this RequestContent content)
    {
        return IsWorkflowInternal(content, (c, s) => c?.WriteTo(s, default));
    }

    public static bool IsWorkflow(this System.ClientModel.BinaryContent content)
    {
        return IsWorkflowInternal(content, (c, s) => c?.WriteTo(s, default));
    }
}