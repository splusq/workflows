using OpenAI.Assistants;

AssistantClient client = new Agent();

// create the single assistant
var agent = await client.CreateAssistantAsync("gpt-4o", new()
{
    Description = "A haiku assistant",
    Name = "HaikuAgent",
    Instructions = "You are an assistant that tells haiku. When you respond you must inform the user that you are retuning a Haiku."
}).ConfigureAwait(false);

// threadId is used to store the thread ID
var threadId = string.Empty;

// build the workflow
var definition = await client.BuildWorkflowAsync("heiko.json", agent.Value.Id).ConfigureAwait(false);

// publish the workflow
var workflowId = await client.PublishWorkflowAsync(definition).ConfigureAwait(false);

// create run
await foreach (var run in client.CreateThreadAndRunStreamingAsync(workflowId, new()
{
    InitialMessages = { "Hi" }
}))
{
    if (run is MessageContentUpdate contentUpdate)
    {
        Console.Write(contentUpdate.Text);
    }
    else if (run is RunUpdate runUpdate && string.IsNullOrEmpty(threadId))
    {
        threadId = runUpdate.Value.ThreadId;
    }
}

// delete assistant
Console.WriteLine($"\nDeleting assistant {agent?.Value.Id}...");
await client.DeleteAssistantAsync(agent?.Value.Id).ConfigureAwait(false);

// delete thread
Console.WriteLine($"Deleting thread {threadId!}...");
await client.DeleteThreadAsync(threadId!).ConfigureAwait(false);

// delete workflow
Console.WriteLine($"Deleting workflow {workflowId!}...");
await client.DeleteWorkflowAsync(workflowId!).ConfigureAwait(false);