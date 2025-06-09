using Azure.AI.Agents.Persistent;
public static class TwoAgentMathChatWorkflow
{
    public static WorkflowDefinition BuildFluent(PersistentAgent studentAgent, PersistentAgent teacherAgent)
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
                            .SetAgent(studentAgent.Id, studentAgent.Name)
                            .WithThread(studentThread)
                            .WithInputMessages(teacherMessages)
                            .WithMaxTurns(5)
                            .WithHumanInLoop(HumanInLoopMode.Never)
                            .OnComplete(builder =>
                                builder.SetMessagesVariable(studentMessages, AgentActorMessageSource.OutputMessages))))
                .AddState(out var teacherState, name: "teacher", description: "The teacher state", stateBuilder =>
                    stateBuilder.AddAgentActor(actorBuilder =>
                        actorBuilder
                            .SetAgent(teacherAgent.Id, teacherAgent.Name)
                            .WithThread(teacherThread)
                            .WithInputMessages(studentMessages)
                            .WithMaxTurns(5)
                            .WithHumanInLoop(HumanInLoopMode.Never)
                            .OnComplete(c =>
                                c.SetMessagesVariable(teacherMessages, AgentActorMessageSource.OutputMessages))))
                .AddFinalState(out var finalState)
                .AddTransitionsForState(studentState, stateTransitionBuilder =>
                    stateTransitionBuilder
                        .ByDefaultTransitionTo(to: teacherState))
                .AddTransitionsForState(teacherState, stateTransitionBuilder =>
                    stateTransitionBuilder
                        .OnConditionTransitionTo(condition: SimpleCondition.Contains(variable: teacherMessages, value: "[COMPLETE]"), finalState)
                        .ByDefaultTransitionTo(to: studentState))
                .WithStartState(studentState)
                .Build();
    }

    public static WorkflowDefinition Build(PersistentAgent studentAgent, PersistentAgent teacherAgent)
    {
        var workflowBuilder =
            new WorkflowBuilder(name: "two_agent_math_chat", description: "2 way chat between student and teacher")
                .AddMessagesVariable(out var studentMessages, name: "studentMessages", description: "Messages from the student")
                .AddThreadVariable(out var studentThread, name: "studentThread", description: "Thread for the student messages")
                .AddMessagesVariable(out var teacherMessages, name: "teacherMessages", description: "Messages from the teacher")
                .AddThreadVariable(out var teacherThread, name: "teacherThread", description: "Thread for the teacher messages");

        var studentActor = new AgentActorBuilder()
            .SetAgent(studentAgent.Id, studentAgent.Name)
            .WithThread(studentThread)
            .WithInputMessages(teacherMessages)
            .WithMaxTurns(5)
            .WithHumanInLoop(HumanInLoopMode.Never)
            .OnComplete(builder =>
                builder.SetMessagesVariable(studentMessages, AgentActorMessageSource.OutputMessages))
            .Build();
        var studentState = new StateBuilder("student", "The student state").AddAgentActor(studentActor).Build();
        workflowBuilder.AddState(out var studentStateRef, studentState);

        var teacherActor = new AgentActorBuilder()
            .SetAgent(teacherAgent.Id, teacherAgent.Name)
            .WithThread(teacherThread)
            .WithInputMessages(studentMessages)
            .WithMaxTurns(5)
            .WithHumanInLoop(HumanInLoopMode.Never)
            .OnComplete(c =>
                c.SetMessagesVariable(teacherMessages, AgentActorMessageSource.OutputMessages))
            .Build();
        var teacherState = new StateBuilder("teacher", "The teacher state").AddAgentActor(teacherActor).Build();
        workflowBuilder.AddState(out var teacherStateRef, teacherState);

        workflowBuilder.AddFinalState(out var finalState);

        var studentTransitions = new StateTransitionBuilder(studentStateRef)
            .ByDefaultTransitionTo(to: teacherStateRef)
            .Build();
        workflowBuilder.AddTransitions(studentTransitions);

        var teacherTransitions = new StateTransitionBuilder(teacherStateRef)
            .OnConditionTransitionTo(condition: SimpleCondition.Contains(variable: teacherMessages, value: "[COMPLETE]"), finalState)
            .ByDefaultTransitionTo(to: studentStateRef)
            .Build();
        workflowBuilder.AddTransitions(teacherTransitions);

        return workflowBuilder
                .WithStartState(studentStateRef)
                .Build();
    }
}