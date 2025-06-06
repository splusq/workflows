using System.CommandLine;
using Azure.AI.Agents.Persistent;
using Azure.Identity;

var endpointOption = new Option<string>("--endpoint", description: "The service endpoint URI.") { IsRequired = true };
var apiVersionOption = new Option<string>("--apiVersion", description: "The API version.") { IsRequired = true };

var rootCommand = new RootCommand
{
    endpointOption,
    apiVersionOption
};

rootCommand.SetHandler(async (string endpoint, string apiVersion) =>
{
    var client = new PersistentAgentsClient(endpoint.TrimEnd('/'), new DefaultAzureCredential(), new PersistentAgentsAdministrationClientOptions().WithPolicy(endpoint, apiVersion));

    // create the single agents
    var teacherAgent = await client.Administration.CreateAgentAsync(
        model: "gpt-4o",
        name: "Teacher",
        instructions: "You are a teacher that create pre-school math question for student and check answer.\nIf the answer is correct, you stop the conversation by saying [COMPLETE].\nIf the answer is wrong, you ask student to fix it."
    );
    Console.WriteLine($"Creating agent {teacherAgent.Value.Name} ({teacherAgent.Value.Id})...");

    var studentAgent = await client.Administration.CreateAgentAsync(
        model: "gpt-4o",
        name: "Student",
        instructions: "You are a student that answer question from teacher, when teacher gives you question you answer them."
    );

    Console.WriteLine($"Creating agent {studentAgent.Value.Name} ({studentAgent.Value.Id})...");

    Workflow? workflow = null;

    try
    {
        // publish the workflow
        workflow = await client.Administration.Pipeline.PublishWorkflowAsync(TwoAgentMathChatWorkflow.Build(studentAgent.Value, teacherAgent.Value));

        // threadId is used to store the thread ID
        PersistentAgentThread thread = await client.Threads.CreateThreadAsync();
        PersistentThreadMessage message = await client.Messages.CreateMessageAsync(
            threadId: thread.Id,
            MessageRole.User,
            content: "Go"
        );

        // create run
        await foreach (var run in client.Runs.CreateRunStreamingAsync(thread.Id, workflow.Id))
        {
            if (run is MessageContentUpdate contentUpdate)
            {
                Console.Write(contentUpdate.Text);
            }
            else if (run is RunUpdate runUpdate)
            {
                if (runUpdate.UpdateKind == StreamingUpdateReason.RunInProgress && !runUpdate.Value.Id.StartsWith("wf_run"))
                {
                    Console.WriteLine();
                    Console.Write($"{runUpdate.Value.Metadata["x-agent-name"]}> ");
                }
            }
        }

        // delete thread, so we can start over
        Console.WriteLine($"\nDeleting thread {thread?.Id}...");
        await client.Threads.DeleteThreadAsync(thread?.Id);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
    finally
    {
        Console.WriteLine();

        // delete agent
        Console.WriteLine($"Deleting agent {teacherAgent?.Value.Name} {teacherAgent?.Value.Id}...");
        await client.Administration.DeleteAgentAsync(teacherAgent?.Value.Id);

        // // delete agent
        Console.WriteLine($"Deleting agent {studentAgent?.Value.Name} {studentAgent?.Value.Id}...");
        await client.Administration.DeleteAgentAsync(studentAgent?.Value.Id);

        // // delete workflow
        Console.WriteLine($"Deleting workflow {workflow?.Id}...");
        await client.Administration.Pipeline.DeleteWorkflowAsync(workflow!);
    }
}, endpointOption, apiVersionOption);

return await rootCommand.InvokeAsync(args);