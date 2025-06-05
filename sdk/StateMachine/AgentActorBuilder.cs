using System.Text.Json.Nodes;

public enum HumanInLoopMode
{
    /// <summary>
    /// Waits for a human to provide input on the chat if no new messages are available for the agent to operate on.
    /// </summary>
    OnNoMessage = 0,

    /// <summary>
    /// Never waits for human input, the agent will continue processing without human intervention.
    /// </summary>
    Never,

    /// <summary>
    /// Always waits for human input before proceeding with the next action.
    /// </summary>
    Always
}

/// <summary>
/// An agent actor in a workflow state.
/// </summary>
public sealed class AgentActor
{
    private readonly string _id;
    private readonly List<string> _messagesIn = new List<string>();
    private readonly Dictionary<string, string> _inputs = new Dictionary<string, string>();
    private readonly AgentActorOnEnterActionsBuilder _agentActorOnEnterActionsBuilder = new AgentActorOnEnterActionsBuilder();
    private readonly AgentActorOnCompleteActionsBuilder _agentActorOnCompleteActionsBuilder = new AgentActorOnCompleteActionsBuilder();

    private string? _agent;
    private string? _agentId;
    private string? _thread;
    private HumanInLoopMode _humanInLoopMode = HumanInLoopMode.OnNoMessage;
    private bool _streamOutput = true;
    private int? _maxTurn;

    /// <summary>
    /// Creates a new instance of the <see cref="AgentActor"/> class.
    /// </summary>
    public AgentActor()
    {
        _id = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Sets the agent for this actor.
    /// </summary>
    /// <param name="agentId">The ID of the agent.</param>
    /// <param name="agentName">The name of the agent.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor SetAgent(string agentId, string? agentName = null)
    {
        if (string.IsNullOrEmpty(agentId))
        {
            throw new ArgumentException("Agent ID cannot be null or empty", nameof(agentId));
        }

        this._agent = agentName;
        this._agentId = agentId;
        return this;
    }

    /// <summary>
    /// Sets the agent name for this actor.
    /// </summary>
    /// <param name="agentName">The name of the agent.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor SetAgentName(string agentName)
    {
        if (string.IsNullOrEmpty(agentName))
        {
            throw new ArgumentException("Agent name cannot be null or empty", nameof(agentName));
        }

        this._agent = agentName;
        return this;
    }

    /// <summary>
    /// Sets the agent for this actor.
    /// </summary>
    /// <param name="dynamicAgentId">The ID of the agent dynamically resolved at run-time from the value stored in the variable.</param>
    /// <param name="dynamicAgentName">The name of the agent dynamically resolved at run-time from the value stored in the variable.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor SetAgent(UserDefinedVariableReference dynamicAgentId, UserDefinedVariableReference? dynamicAgentName = null)
    {
        if (dynamicAgentId == null)
        {
            throw new ArgumentNullException(nameof(dynamicAgentId), "Dynamic agent ID cannot be null");
        }

        this._agent = dynamicAgentName?.Name;
        this._agentId = dynamicAgentId.Name;
        return this;
    }

    public AgentActor SetAgentName(UserDefinedVariableReference dynamicAgentName)
    {
        if (dynamicAgentName == null)
        {
            throw new ArgumentNullException(nameof(dynamicAgentName), "Dynamic agent name cannot be null");
        }

        this._agent = dynamicAgentName.Name;
        return this;
    }

    /// <summary>
    /// Sets the thread variable for this actor.
    /// </summary>
    /// <param name="thread">The thread variable reference.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor WithThread(ThreadVariableReference thread)
    {
        this._thread = thread?.Name ?? throw new ArgumentNullException(nameof(thread));
        return this;
    }

    /// <summary>
    /// Adds an input messages variable to this actor.
    /// </summary>
    /// <param name="messages">The messages variable reference.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor WithInputMessages(params MessagesVariableReference[] messages)
    {
        if (messages == null || messages.Length == 0) throw new ArgumentNullException(nameof(messages));
        foreach (var message in messages)
        {
            this._messagesIn.Add(message.Name);
        }
        return this;
    }

    /// <summary>
    /// Adds an input mapping for this actor.
    /// </summary>
    /// <param name="inputName">The name of the input in the actor.</param>
    /// <param name="variable">The variable reference to map to this input.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor WithInput(string inputName, UserDefinedVariableReference variable)
    {
        if (string.IsNullOrEmpty(inputName)) throw new ArgumentException("Input name cannot be null or empty", nameof(inputName));
        if (variable == null) throw new ArgumentNullException(nameof(variable));

        this._inputs[inputName] = variable.Name;
        return this;
    }

    /// <summary>
    /// Configures actions to be executed when entering the state.
    /// </summary>
    /// <param name="configure">The builder action to configure the actions.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor OnEnter(Action<AgentActorOnEnterActionsBuilder> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        configure(this._agentActorOnEnterActionsBuilder);
        return this;
    }

    /// <summary>
    /// Configures actions to be executed when the actor completes.
    /// </summary>
    /// <param name="configure">The builder action to configure the actions.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor OnComplete(Action<AgentActorOnCompleteActionsBuilder> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        configure(this._agentActorOnCompleteActionsBuilder);
        return this;
    }

    /// <summary>
    /// Sets the human-in-loop mode for this actor.
    /// </summary>
    /// <param name="mode">The human-in-loop mode.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor WithHumanInLoop(HumanInLoopMode mode)
    {
        this._humanInLoopMode = mode;
        return this;
    }

    /// <summary>
    /// Disables output message streaming. Use this to prevent the agent from streaming output messages to the thread/chat.
    /// </summary>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor DisableOutputMessageStreaming()
    {
        this._streamOutput = false;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of turns for this actor.
    /// </summary>
    /// <param name="maxTurn">The maximum number of turns.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActor WithMaxTurns(uint maxTurn)
    {
        if (maxTurn <= 0 || maxTurn > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTurn), "Max turns must be greater than zero and less then 10.");
        }
        this._maxTurn = (int)maxTurn;
        return this;
    }

    /// <summary>
    /// Builds the agent actor.
    /// </summary>
    /// <returns>The completed agent actor.</returns>
    internal JsonObject ToJson()
    {
        if (string.IsNullOrEmpty(this._agent) && string.IsNullOrEmpty(this._agentId))
        {
            throw new InvalidOperationException("Agent or AgentId must be set for an actor.");
        }

        var agentActor  = new JsonObject
        {
            ["id"] = this._id,
            ["humanInLoopMode"] = this._humanInLoopMode.ToString(),
            ["streamOutput"] = this._streamOutput
        };

        if (this._agent != null)
        {
            agentActor["agent"] = this._agent;
        }

        if (this._agentId != null)
        {
            agentActor["agentId"] = this._agentId;
        }

        if (this._thread != null)
        {
            agentActor["thread"] = this._thread;
        }

        if (this._maxTurn != null)
        {
            agentActor["maxTurn"] = this._maxTurn.Value;
        }

        if (this._messagesIn.Count > 0)
        {
            var messagesIn = new JsonArray();
            foreach (var message in _messagesIn)
            {
                messagesIn.Add(message);
            }

            agentActor["messagesIn"] = messagesIn;
        }

        if (this._inputs.Count > 0)
        {
            var inputs = new JsonObject();
            foreach (var input in this._inputs)
            {
                inputs[input.Key] = input.Value;
            }
            agentActor["inputs"] = inputs;
        }

        this._agentActorOnEnterActionsBuilder.BuildJsontWith(agentActor);
        this._agentActorOnCompleteActionsBuilder.BuildJsonWith(agentActor);

        return agentActor;
    }
}