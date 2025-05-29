using System.CommandLine;
using OpenAI.Assistants;

var endpointOption = new Option<string>("--endpoint", description: "The service endpoint URI.") { IsRequired = true };
var audienceOption = new Option<string>("--audience", description: "The audience for authentication.") { IsRequired = true };
var apiVersionOption = new Option<string>("--apiVersion", description: "The API version.") { IsRequired = true };

var rootCommand = new RootCommand
{
    endpointOption,
    audienceOption,
    apiVersionOption
};

rootCommand.SetHandler(async (string endpoint, string audience, string apiVersion) =>
{
    AssistantClient client = new AssistantClientWithOptions(endpoint, audience, apiVersion);

    // create the single agents
    var teacherAgent = await client.CreateAssistantAsync("gpt-4o", new()
    {
        Description = "A math teacher assistant",
        Name = "Teacher",
        Instructions = "You are a teacher that create pre-school math question for student and check answer.\nIf the answer is correct, you stop the conversation by saying [COMPLETE].\nIf the answer is wrong, you ask student to fix it."
    });
    Console.WriteLine($"Creating agent {teacherAgent.Value.Name} ({teacherAgent.Value.Id})...");

    var studentAgent = await client.CreateAssistantAsync("gpt-4o", new()
    {
        Description = "A student assistant",
        Name = "Student",
        Instructions = "You are a student that answer question from teacher, when teacher gives you question you answer them."
    });

    Console.WriteLine($"Creating agent {studentAgent.Value.Name} ({studentAgent.Value.Id})...");

    Workflow? workflow = null;

    try
    {
        // publish the workflow
        workflow = await client.Pipeline.PublishWorkflowAsync(Workflows.Build<TwoAgentMathState>(studentAgent.Value.Id, studentAgent.Value.Name, teacherAgent.Value.Id, teacherAgent.Value.Name));

        // threadId is used to store the thread ID
        var threadId = string.Empty;

        // create run
        await foreach (var run in client.CreateThreadAndRunStreamingAsync(workflow.Id, new()
        {
            InitialMessages = { "Go" }
        }))
        {
            if (run is MessageContentUpdate contentUpdate)
            {
                Console.Write(contentUpdate.Text);
            }
            else if (run is RunUpdate runUpdate)
            {
                if (threadId == string.Empty)
                {
                    threadId = runUpdate.Value.ThreadId;
                }
                if (runUpdate.UpdateKind == StreamingUpdateReason.RunInProgress && !runUpdate.Value.Id.StartsWith("wf_run"))
                {
                    Console.WriteLine();
                    Console.Write($"{runUpdate.Value.Metadata["x-agent-name"]}> ");
                }
            }
        }

        // delete thread, so we can start over
        Console.WriteLine($"\nDeleting thread {threadId!}...");
        await client.DeleteThreadAsync(threadId!);
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
        await client.DeleteAssistantAsync(teacherAgent?.Value.Id);

        // delete agent
        Console.WriteLine($"Deleting agent {studentAgent?.Value.Name} {studentAgent?.Value.Id}...");
        await client.DeleteAssistantAsync(studentAgent?.Value.Id);

        // delete workflow
        Console.WriteLine($"Deleting workflow {workflow?.Id}...");
        await client.Pipeline.DeleteWorkflowAsync(workflow!);
    }
}, endpointOption, audienceOption, apiVersionOption);

return await rootCommand.InvokeAsync(args);