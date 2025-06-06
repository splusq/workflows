/// <summary>
/// A workflow state.
/// </summary>
public sealed class StateBuilder
{
    private readonly string? _description;
    private readonly List<AgentActor> _actors = new List<AgentActor>();
    private bool _isFinal;

    /// <summary>
    /// Creates a new instance of the <see cref="StateBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="description">The description of the state.</param>
    public StateBuilder(string name, string? description = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        this.Name = name;
        this._description = description;
    }

    public string Name { get; }

    /// <summary>
    /// Adds an agent actor to the state.
    /// </summary>
    /// <param name="configure">The builder action to configure the actor.</param>
    /// <returns>The updated <see cref="StateBuilder"/> instance.</returns>
    public StateBuilder AddAgentActor(Action<AgentActorBuilder> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var builder = new AgentActorBuilder();
        configure(builder);
        this._actors.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Adds an agent actor to the state.
    /// </summary>
    /// <param name="configure">The builder action to configure the actor.</param>
    /// <returns>The updated <see cref="StateBuilder"/> instance.</returns>
    public StateBuilder AddAgentActor(AgentActor actor)
    {
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        this._actors.Add(actor);
        return this;
    }

    /// <summary>
    /// Marks the state as a final state.
    /// </summary>
    /// <returns>The updated <see cref="StateBuilder"/> instance.</returns>
    public StateBuilder MarkAsFinal()
    {
        this._isFinal = true;
        return this;
    }

    public State Build()
    {          
        return new State
        {
            Name = this.Name,
            Description = this._description,
            IsFinal = this._isFinal,
            Actors = this._actors
        };
    }
}