public static class Workflows
{
    public static WorkflowBuilder Build(string studentAgentId, string studentAgentName, string teacherAgentId, string teacherAgentName)
    {
        return
            new WorkflowBuilder(name: "two_agent_math_chat", description: "2 way chat between student and teacher")
                .AddMessagesVariable(out var studentMessages, name: "studentMessages", description: "Messages from the student")
                .AddThreadVariable(out var studentThread, name: "studentThread", description: "Thread for the student messages")
                .AddMessagesVariable(out var teacherMessages, name: "teacherMessages", description: "Messages from the teacher")
                .AddThreadVariable(out var teacherThread, name: "teacherThread", description: "Thread for the teacher messages")
                .AddState(out var studentState, name: "student", description: "The student state", stateBuilder =>
                    stateBuilder.AddAgentActor(actorBuilder =>
                        actorBuilder
                            .SetAgentName(studentAgentName)
                            .WithThread(studentThread)
                            .WithInputMessages(teacherMessages)
                            .WithMaxTurns(5)
                            .WithHumanInLoop(HumanInLoopMode.Never)
                            .OnComplete(builder =>
                                builder.SetMessagesVariable(studentMessages, AgentActorMessageSource.OutputMessages))))
                .AddState(out var teacherState, name: "teacher", description: "The teacher state", stateBuilder =>
                    stateBuilder.AddAgentActor(actorBuilder =>
                        actorBuilder
                            .SetAgentName(teacherAgentName)
                            .WithThread(teacherThread)
                            .WithInputMessages(studentMessages)
                            .WithMaxTurns(5)
                            .WithHumanInLoop(HumanInLoopMode.Never)
                            .OnComplete(c =>
                                c.SetMessagesVariable(teacherMessages, AgentActorMessageSource.OutputMessages))))
                .AddFinalState(out var finalState)
                .WithStartState(studentState)
                .ForState(studentState, stateTransitionBuilder =>
                    stateTransitionBuilder
                        .ByDefaultTransitionTo(to: teacherState))
                .ForState(teacherState, stateTransitionBuilder =>
                    stateTransitionBuilder
                        .OnConditionTransitionTo(condition: SimpleCondition.Contains(variable: teacherMessages, value: "[COMPLETE]"), finalState)
                        .ByDefaultTransitionTo(to: studentState));
    }
}