using System.Text.Json.Nodes;


/// <summary>
/// A workflow state.
/// </summary>
public sealed class State
{
    private readonly string? _description;
    private readonly List<AgentActor> _actors = new List<AgentActor>();
    private bool _isFinal;

    /// <summary>
    /// Creates a new instance of the <see cref="State"/> class.
    /// </summary>
    /// <param name="name">The name of the state.</param>
    /// <param name="description">The description of the state.</param>
    public State(string name, string? description = null)
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
    /// <returns>The updated <see cref="State"/> instance.</returns>
    public State AddAgentActor(Action<AgentActor> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var actor = new AgentActor();
        configure(actor);
        this._actors.Add(actor);
        return this;
    }

    /// <summary>
    /// Adds an agent actor to the state.
    /// </summary>
    /// <param name="configure">The builder action to configure the actor.</param>
    /// <returns>The updated <see cref="State"/> instance.</returns>
    public State AddAgentActor(AgentActor actor)
    {
        if (actor == null) throw new ArgumentNullException(nameof(actor));
        this._actors.Add(actor);
        return this;
    }

    /// <summary>
    /// Marks the state as a final state.
    /// </summary>
    /// <returns>The updated <see cref="State"/> instance.</returns>
    public State MarkAsFinal()
    {
        this._isFinal = true;
        return this;
    }

    /// <summary>
    /// Builds the state as json.
    /// </summary>
    /// <returns>The state.</returns>
    internal JsonObject ToJson()
    {
        if (this._actors.Count == 0 && !this._isFinal)
        {
            throw new InvalidOperationException("At least one actor must be defined for a non-final state.");
        }

        var state = new JsonObject
        {
            ["name"] = Name
        };

        if (this._description != null)
        {
            state["description"] = this._description;
        }

        if (this._isFinal)
        {
            state["isFinal"] = true;
        }

        if (this._actors.Any())
        {
            var actors = new JsonArray();
            foreach (var actor in this._actors)
            {
                actors.Add(actor.ToJson());
            }
            state["actors"] = actors;
        }

        return state;
    }
}