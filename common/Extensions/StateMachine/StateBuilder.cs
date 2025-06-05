using System.Text.Json.Nodes;


/// <summary>
/// Builder for defining workflow states.
/// </summary>
public sealed class StateBuilder
{
    private readonly string? _description;
    private readonly List<JsonObject> _actors = new List<JsonObject>();
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

        var builder = new AgentActorBuilder(Guid.NewGuid().ToString("N"));
        configure(builder);
        var actor = builder.BuildJson();
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

    /// <summary>
    /// Builds the state.
    /// </summary>
    /// <returns>The completed state.</returns>
    internal JsonObject BuildJson()
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
                actors.Add(actor);
            }
            state["actors"] = actors;
        }

        return state;
    }
}