/// <summary>
/// An agent actor in a workflow state.
/// </summary>
public sealed class AgentActorBuilder
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
    private int? _maxTransientErrorRetries;
    private int? _maxRateLimitRetries;

    /// <summary>
    /// Creates a new instance of the <see cref="AgentActorBuilder"/> class.
    /// </summary>
    public AgentActorBuilder()
    {
        this._id = Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Sets the agent for this actor.
    /// </summary>
    /// <param name="agentId">The ID of the agent.</param>
    /// <param name="agentName">The name of the agent.</param>
    /// <returns>The updated <see cref="AgentActorBuilder"/> instance.</returns>
    public AgentActorBuilder SetAgent(string agentId, string? agentName = null)
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
    public AgentActorBuilder SetAgentName(string agentName)
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
    public AgentActorBuilder SetAgent(UserDefinedVariableReference dynamicAgentId, UserDefinedVariableReference? dynamicAgentName = null)
    {
        if (dynamicAgentId == null)
        {
            throw new ArgumentNullException(nameof(dynamicAgentId), "Dynamic agent ID cannot be null");
        }

        this._agent = dynamicAgentName?.Name;
        this._agentId = dynamicAgentId.Name;
        return this;
    }

    public AgentActorBuilder SetAgentName(UserDefinedVariableReference dynamicAgentName)
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
    public AgentActorBuilder WithThread(ThreadVariableReference thread)
    {
        this._thread = thread?.Name ?? throw new ArgumentNullException(nameof(thread));
        return this;
    }

    /// <summary>
    /// Adds an input messages variable to this actor.
    /// </summary>
    /// <param name="messages">The messages variable reference.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActorBuilder WithInputMessages(params MessagesVariableReference[] messages)
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
    public AgentActorBuilder WithInput(string inputName, UserDefinedVariableReference variable)
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
    public AgentActorBuilder OnEnter(Action<AgentActorOnEnterActionsBuilder> configure)
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
    public AgentActorBuilder OnComplete(Action<AgentActorOnCompleteActionsBuilder> configure)
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
    public AgentActorBuilder WithHumanInLoop(HumanInLoopMode mode)
    {
        this._humanInLoopMode = mode;
        return this;
    }

    /// <summary>
    /// Disables output message streaming. Use this to prevent the agent from streaming output messages to the thread/chat.
    /// </summary>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActorBuilder DisableOutputMessageStreaming()
    {
        this._streamOutput = false;
        return this;
    }

    /// <summary>
    /// Sets the maximum number of turns for this actor.
    /// </summary>
    /// <param name="maxTurn">The maximum number of turns.</param>
    /// <returns>The updated <see cref="AgentActor"/> instance.</returns>
    public AgentActorBuilder WithMaxTurns(uint maxTurn)
    {
        if (maxTurn <= 0 || maxTurn > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTurn), "Max turns must be greater than zero and less then 10.");
        }
        this._maxTurn = (int)maxTurn;
        return this;
    }

    /// <summary>
    /// Configures the builder to retry operations in the event of transient errors or rate limits.
    /// </summary>
    /// <param name="maxTransientErrorRetries">The maximum number of retries to attempt for transient errors.</param>
    /// <param name="maxRateLimitRetries">The maximum number of retries to attempt when rate limits are encountered.</param>
    /// <returns>The current <see cref="AgentActorBuilder"/> instance with the retry configuration applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxTransientErrorRetries"/> or <paramref name="maxRateLimitRetries"/> is less than 0.</exception>
    public AgentActorBuilder WithRetries(int? maxTransientErrorRetries, int? maxRateLimitRetries)
    {
        if (maxTransientErrorRetries != null && maxTransientErrorRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTransientErrorRetries), "Must be greater than or equal to zero.");
        }
        if (maxRateLimitRetries != null && maxRateLimitRetries < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRateLimitRetries), "Must be greater than or equal to zero.");
        }

        this._maxTransientErrorRetries = maxTransientErrorRetries;
        this._maxRateLimitRetries = maxRateLimitRetries;
        return this;
    }

    public AgentActor Build()
    {
        var agentActor = new AgentActor
        {
            Id = this._id,
            Agent = this._agent,
            AgentId = this._agentId,
            Thread = this._thread,
            HumanInLoopMode = this._humanInLoopMode,
            StreamOutput = this._streamOutput,
            MaxTurn = this._maxTurn,
            MaxRateLimitRetries = this._maxRateLimitRetries,
            MaxTransientErrorRetries = this._maxTransientErrorRetries,
            MessagesIn = this._messagesIn.Any() ? this._messagesIn : null,
            Inputs = this._inputs.Any() ? this._inputs : null,
        };

        agentActor = this._agentActorOnEnterActionsBuilder.Build(agentActor);
        agentActor = this._agentActorOnCompleteActionsBuilder.Build(agentActor);
        return agentActor;
    }
}