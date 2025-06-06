using Azure.Core;
using Azure.Core.Pipeline;
using sdk.Agent;
using System.ClientModel.Primitives;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public static class Ext
{
    public static async Task<AgentExtensions> UpdateAgentExtensionsAsync(this ClientPipeline pipeline, string agentId, AgentExtensionsDefinition extensions)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = "POST";
        message.Request.Uri = new Uri($"https://localhost/assistants/{agentId}/extensions");
        message.Request.Content = System.ClientModel.BinaryContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(extensions.ToJson())));
        message.Request.Headers.Add("Content-Type", "application/json");
        await pipeline.SendAsync(message).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error updating agent extensions: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return AgentExtensions.FromJson(responseJson);
    }

    public static async Task<AgentExtensions> GetAgentExtensionsAsync(this ClientPipeline pipeline, string agentId)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = "GET";
        message.Request.Uri = new Uri($"https://localhost/assistants/{agentId}/extensions");
        await pipeline.SendAsync(message).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error getting agent extensions: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return AgentExtensions.FromJson(responseJson);
    }

    public static async Task<RunOutputs> GetAgentOutputsAsync(this ClientPipeline pipeline, string threadId, string runId)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = "GET";
        message.Request.Uri = new Uri($"https://localhost/threads/{threadId}/runs/{runId}/outputs");
        await pipeline.SendAsync(message).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error getting run outputs: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return RunOutputs.FromJson(responseJson);
    }

    public static async Task<RunEvents> GetAgentEventsAsync(this ClientPipeline pipeline, string threadId, string runId)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = "GET";
        message.Request.Uri = new Uri($"https://localhost/threads/{threadId}/runs/{runId}/events");
        await pipeline.SendAsync(message).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error getting run events: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return RunEvents.FromJson(responseJson);
    }

    public static async Task<AgentExtensions> UpdateAgentExtensionsAsync(this HttpPipeline pipeline, string agentId, AgentExtensionsDefinition extensions)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Post;
        message.Request.Uri.Reset(new Uri($"https://localhost/assistants/{agentId}/extensions"));
        message.Request.Content = RequestContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(extensions.ToJson())));
        message.Request.Headers.Add("Content-Type", "application/json");


        await pipeline.SendAsync(message, default).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error updating agent extensions: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return AgentExtensions.FromJson(responseJson);
    }

    public static async Task<AgentExtensions> GetAgentExtensionsAsync(this HttpPipeline pipeline, string agentId)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Get;
        message.Request.Uri.Reset(new Uri($"https://localhost/assistants/{agentId}/extensions"));
        await pipeline.SendAsync(message, default).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error getting agent extensions: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return AgentExtensions.FromJson(responseJson);
    }

    public static async Task<RunOutputs> GetAgentOutputsAsync(this HttpPipeline pipeline, string threadId, string runId)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Get;
        message.Request.Uri.Reset(new Uri($"https://localhost/threads/{threadId}/runs/{runId}/outputs"));
        await pipeline.SendAsync(message, default).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error getting run outputs: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return RunOutputs.FromJson(responseJson);
    }

    public static async Task<RunEvents> GetAgentEventsAsync(this HttpPipeline pipeline, string threadId, string runId)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Get;
        message.Request.Uri.Reset(new Uri($"https://localhost/threads/{threadId}/runs/{runId}/events"));
        await pipeline.SendAsync(message, default).ConfigureAwait(false);
        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);
            throw new Exception($"Error getting run events: {errorContent}");
        }
        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return RunEvents.FromJson(responseJson);
    }

    public static async Task<Workflow> PublishWorkflowAsync(this ClientPipeline pipeline, WorkflowDefinition workflowDefinition, string? id = null)
    {
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = "POST";
        message.Request.Uri = new Uri("https://localhost/agents" + (id != null ? $"/{id}" : string.Empty));
        message.Request.Content = System.ClientModel.BinaryContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(workflowDefinition.ToJson())));
        message.Request.Headers.Add("Content-Type", "application/json");

        await pipeline.SendAsync(message).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);

            throw new Exception($"Error publishing workflow: {errorContent}");
        }

        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return Workflow.FromJson(responseJson);
    }

    public static async Task<Workflow> PublishWorkflowAsync(this HttpPipeline pipeline, WorkflowDefinition workflowDefinition, string? id = null)
    {
        var json = workflowDefinition.ToJson();
        Console.WriteLine($"Publishing workflow: {json}");
        Console.WriteLine($"Publishing workflow: {workflowDefinition.ToYaml()}");
        // Send the request
        using var message = pipeline.CreateMessage();
        message.Request.Method = RequestMethod.Post;
        message.Request.Uri.Reset(new Uri("https://localhost/agents" + (id != null ? $"/{id}" : string.Empty)));
        message.Request.Content = RequestContent.Create(new MemoryStream(Encoding.UTF8.GetBytes(workflowDefinition.ToJson())));
        message.Request.Headers.Add("Content-Type", "application/json");

        await pipeline.SendAsync(message, default).ConfigureAwait(false);

        if (message.Response?.Status < 200 || message.Response?.Status >= 300)
        {
            var errorContent = await message.Response.Content.AsJsonAsync().ConfigureAwait(false);

            throw new Exception($"Error publishing workflow: {errorContent}");
        }

        var responseJson = await message.Response!.Content.AsJsonAsync().ConfigureAwait(false) ?? string.Empty;
        return Workflow.FromJson(responseJson);
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

        bool isAgentExtensions =
            (uri.AbsolutePath.Contains("/assistants") && uri.AbsolutePath.EndsWith("/extensions"))
            || uri.AbsolutePath.Contains("/runs") && (uri.AbsolutePath.EndsWith("/events") || uri.AbsolutePath.EndsWith("/outputs"));

        bool shouldRewriteToWorkflow =
            (isRunOrAgentPath && isWorkflow) || isWorkflowInstance || isAgentExtensions;

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
        if (data == null)
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
            return StreamContainsWorkflowPattern(stream, @"""assistant_id"":""wf_", @"""workflow_version", @"""startState"":");
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