using OpenAI.Assistants;

AssistantClient client = new Agent();

// create the single assistant
var agent = await client.CreateAssistantAsync("gpt-4o", new AssistantCreationOptions()
{
    Description = "A haiku assistant",
    Name = "HaikuAgent",
    Instructions = "You are an assistant that tells haiku. When you respond you must inform the user that you are retuning a Haiku."
}).ConfigureAwait(false);

// create thread
var thread = await client.CreateThreadAsync().ConfigureAwait(false);
var workflowId = string.Empty;

try 
{
    // build the workflow
    var definition = await client.BuildWorkflowAsync("heiko.json", agent.Value.Id).ConfigureAwait(false);

    // publish the workflow
    workflowId = await client.PublishWorkflowAsync(definition).ConfigureAwait(false);

    // create run
    await foreach (var run in client.CreateRunStreamingAsync(thread.Value.Id, workflowId))
    {
        Console.WriteLine($"event: {run}");
        if (run is MessageContentUpdate contentUpdate)
        {
            Console.Write(contentUpdate.Text);
        }
    }
}
catch (Exception e) 
{
    Console.WriteLine($"Exception: {e}");
}
finally
{
    // delete assistant
    Console.WriteLine($"Deleting assistant {agent?.Value.Id}...");
    await client.DeleteAssistantAsync(agent?.Value.Id).ConfigureAwait(false);

    // delete thread
    Console.WriteLine($"Deleting thread {thread?.Value.Id}...");
    await client.DeleteThreadAsync(thread?.Value.Id).ConfigureAwait(false);

    // delete workflow
    Console.WriteLine($"Deleting workflow {workflowId!}...");
    await client.DeleteWorkflowAsync(workflowId!).ConfigureAwait(false);
}