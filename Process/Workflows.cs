using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using OpenAI.Assistants;

public static class Workflows
{
    public class TwoAgentMathState
    {
        public List<ChatMessageContent>? UserMessages { get; set; }

        public List<ChatMessageContent>? StudentMessages { get; set; }

        public List<ChatMessageContent>? TeacherMessages { get; set; }
    }

    public static KernelProcess BuildTutor(Assistant studentAgent, Assistant teacherAgent)
    {
        var studentDefinition = new AgentDefinition { Id = studentAgent.Id, Name = studentAgent.Name, Type = AzureAIAgentFactory.AzureAIAgentType };
        var teacherDefinition = new AgentDefinition { Id = teacherAgent.Id, Name = teacherAgent.Name, Type = AzureAIAgentFactory.AzureAIAgentType };

        // Define the process with a state type
        var processBuilder = new FoundryProcessBuilder<TwoAgentMathState>("two_agent_math_chat");

        // Create a thread for the student
        processBuilder.AddThread("Student", KernelProcessThreadLifetime.Scoped);
        processBuilder.AddThread("Teacher", KernelProcessThreadLifetime.Scoped);

        // Add the student
        var student = processBuilder.AddStepFromAgent(studentDefinition);

        // Add the teacher
        var teacher = processBuilder.AddStepFromAgent(teacherDefinition);

        /**************************** Orchestrate ***************************/

        // When the process starts, activate the student agent
        processBuilder.OnProcessEnter().SendEventTo(
            student,
            thread: "_variables_.Student",
            messagesIn: ["_variables_.TeacherMessages"],
            inputs: new Dictionary<string, string> { });
            
        // When the student agent exits, update the process state to save the student's messages and update interaction counts
        processBuilder.OnStepExit(student)
            .UpdateProcessState(path: "StudentMessages", operation: StateUpdateOperations.Set, value: "_agent_.messages_out");

        // When the student agent is finished, send the messages to the teacher agent
        processBuilder.OnEvent(student, "_default_")
            .SendEventTo(teacher, messagesIn: ["_variables_.StudentMessages"], thread: "Teacher");

        // When the teacher agent exits with a message containing '[COMPLETE]', update the process state to save the teacher's messages and update interaction counts and emit the `correct_answer` event
        processBuilder.OnStepExit(teacher, condition: "jmespath(contains(to_string(_agent_.messages_out), '[COMPLETE]'))")
            .EmitEvent(
                eventName: "correct_answer",
                payload: new Dictionary<string, string>
                {
                    { "Question", "_variables_.TeacherMessages" },
                    { "Answer", "_variables_.StudentMessages" }
                })
            .UpdateProcessState(path: "_variables_.TeacherMessages", operation: StateUpdateOperations.Set, value: "_agent_.messages_out");

        // When the teacher agent exits with a message not containing '[COMPLETE]', update the process state to save the teacher's messages and update interaction counts
        processBuilder.OnStepExit(teacher, condition: "_default_")
            .UpdateProcessState(path: "_variables_.TeacherMessages", operation: StateUpdateOperations.Set, value: "_agent_.messages_out");

        // When the teacher agent is finished, send the messages to the student agent
        processBuilder.OnEvent(teacher, "_default_", condition: "_default_")
            .SendEventTo(student, messagesIn: ["_variables_.TeacherMessages"], thread: "Student");

        // When the teacher agent emits the `correct_answer` event, stop the process
        processBuilder.OnEvent(teacher, "correct_answer")
            .StopProcess();

        return processBuilder.Build();
    }
}