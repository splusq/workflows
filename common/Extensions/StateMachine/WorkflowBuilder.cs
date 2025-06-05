
using System.Text.Json.Nodes;
    
/// <summary>
/// Builder for creating workflow definitions using a fluent API.
/// </summary>
public sealed class WorkflowBuilder
{
    private readonly string _name;
    private readonly string? _description;
    private readonly Dictionary<string, JsonObject> _variablesMap = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, JsonObject> _statesMap = new Dictionary<string, JsonObject>();
    private readonly Dictionary<string, StateTransitionBuilder> _transitionBuilders = new Dictionary<string, StateTransitionBuilder>(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Creates a new instance of the <see cref="WorkflowBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the workflow.</param>
    /// <param name="description">The description of the workflow.</param>
    public WorkflowBuilder(string name, string? description = null)
    {
        this._name = name ?? throw new ArgumentNullException(nameof(name));
        this._description = description;
    }

    /// <summary>
    /// Adds a messages variable to the workflow.
    /// </summary>
    /// <param name="variable">Output parameter that will hold a reference to the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="description">The description of the variable.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when variable name is null, empty, or a variable with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddMessagesVariable(out MessagesVariableReference variable, string name, string description = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (_variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        _variablesMap.Add(name, new JsonObject
        {
            ["type"] = "messages",
            ["name"] = name,
            ["description"] = description
        });
        variable = new MessagesVariableReference(name);
        return this;
    }

    /// <summary>
    /// Adds a thread variable to the workflow.
    /// </summary>
    /// <param name="variable">Output parameter that will hold a reference to the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="description">The description of the variable.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when variable name is null, empty, or a variable with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddThreadVariable(out ThreadVariableReference variable, string name, string description = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (_variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        _variablesMap.Add(name, new JsonObject
        {
            ["type"] = "thread",
            ["name"] = name,
            ["description"] = description
        });
        variable = new ThreadVariableReference(name);
        return this;
    }

    /// <summary>
    /// Adds a user-defined variable to the workflow.
    /// </summary>
    /// <param name="variable">Output parameter that will hold a reference to the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="description">The description of the variable.</param>
    /// <param name="defaultValue">The default value of the variable.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when variable name is null, empty, or a variable with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddUserDefinedVariable(out UserDefinedVariableReference variable, string name, string description = null, JsonNode defaultValue = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (_variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        var jsonObject = new JsonObject
        {
            ["type"] = "userDefined",
            ["name"] = name,
            ["description"] = description
        };

        if (defaultValue != null)
        {
            jsonObject["value"] = defaultValue;
        }

        _variablesMap.Add(name, jsonObject);
        variable = new UserDefinedVariableReference(name);
        return this;
    }

    /// <summary>
    /// Adds a state to the workflow.
    /// </summary>
    /// <param name="state">Output parameter that will hold a reference to the state.</param>
    /// <param name="name">The name of the state.</param>
    /// <param name="description">The description of the state.</param>
    /// <param name="configure">Optional builder action to configure the state.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when state name is null, empty, or a state with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddState(out StateReference state, string name, string description, Action<StateBuilder> configure)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("State name cannot be null or empty", nameof(name));

        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (_statesMap.ContainsKey(name))
            throw new ArgumentException($"A state with the name '{name}' already exists", nameof(name));

        var builder = new StateBuilder(name, description);
        configure(builder);

        var newState = builder.BuildJson();
        _statesMap.Add(name, newState);

        state = new StateReference(name);
        return this;
    }

    public WorkflowBuilder AddFinalState(out StateReference state, string name = "End", string description = "The final state")
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("State name cannot be null or empty", nameof(name));
        if (_statesMap.ContainsKey(name))
            throw new ArgumentException($"A state with the name '{name}' already exists", nameof(name));
        _statesMap.Add(name, new StateBuilder(name, description).MarkAsFinal().BuildJson());
        state = new StateReference(name);
        return this;
    }

    /// <summary>
    /// Creates a transition builder for a specific source state.
    /// </summary>
    /// <param name="source">The source state reference.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    public WorkflowBuilder ForState(StateReference source, Action<StateTransitionBuilder> configure)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (!_transitionBuilders.TryGetValue(source.Name, out var builder))
        {
            builder = new StateTransitionBuilder(source);
            _transitionBuilders[source.Name] = builder;
        }
        configure(builder);
        return this;
    }

    /// <summary>
    /// Builds the workflow definition.
    /// </summary>
    /// <param name="startState">The state that the workflow should start in.</param>
    /// <returns>The completed workflow definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when startState is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no states are defined or when there are no transitions between states.</exception>
    public JsonObject BuildJson(StateReference startState)
    {
        if (startState == null) throw new ArgumentNullException(nameof(startState));

        if (_statesMap.Count == 0)
            throw new InvalidOperationException("At least one state must be defined.");

        var transitions = new JsonArray();
        foreach (var builder in _transitionBuilders.Values)
        {
            var transitionsForState = builder.BuildJson();
            if (transitionsForState.Count > 0)
            {
                foreach (var transition in transitionsForState)
                {
                    if (transition != null)
                    {
                        transitions.Add(transition.DeepClone());
                    }
                }
            }
        }

        if (transitions.Count == 0)
            throw new InvalidOperationException("At least one transition must be defined.");

        var variables = new JsonArray();
        foreach (var variable in _variablesMap.Values)
        {
            variables.Add(variable);
        }

        var _startState = startState.Name;
        var states = new JsonArray();
        foreach (var state in _statesMap.Values)
        {
            states.Add(state.DeepClone());
        }

        var workflow = new JsonObject
        {
            ["name"] = _name,
            ["variables"] = variables,
            ["states"] = states,
            ["startState"] = _startState,
            ["transitions"] = transitions
        };

        if (this._description != null)
        {
            workflow["description"] = _description;
        }

        return workflow;
    }
}