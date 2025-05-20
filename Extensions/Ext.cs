using Azure.Identity;
using OpenAI.Assistants;
using System.Text.Json;
using System.Text;
using Microsoft.SemanticKernel;

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

    public static async Task<string> PublishWorkflowAsync<T>(this AssistantClient client, FoundryProcessBuilder<T> process) where T : class, new()
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

            return workflowId;
        }
    }

    public static async Task DeleteWorkflowAsync(this AssistantClient _, string workflowId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"agents/{workflowId}");
        var response = await _client.SendAsync(request).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
    }

    public static bool IsWorkflow(this System.ClientModel.BinaryContent content)
    {
        // Look for: "assistant_id":"wf_"
        var pattern = Encoding.UTF8.GetBytes("\"assistant_id\":\"wf_");
        using var stream = new MemoryStream();
        content.WriteTo(stream, default);
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

    extension(Console)
    {
        public static async IAsyncEnumerable<string> Readlines(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                {
                    break;
                }

                yield return input;
            }

            await Task.Yield();
        }
    }
}