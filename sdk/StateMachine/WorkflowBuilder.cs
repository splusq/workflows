using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

/// <summary>
/// Builder for creating workflow definitions using a fluent API.
/// </summary>
public sealed class WorkflowBuilder
{
    private readonly string _name;
    private readonly string? _description;
    private readonly Dictionary<string, JsonObject> _variablesMap = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, State> _statesMap = new Dictionary<string, State>();
    private readonly Dictionary<string, StateTransitionBuilder> _transitionBuilders = new Dictionary<string, StateTransitionBuilder>(StringComparer.InvariantCultureIgnoreCase);
    private string? _startState = null;

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
    public WorkflowBuilder AddMessagesVariable(out MessagesVariableReference variable, string name, string? description = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (this._variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        this._variablesMap.Add(name, new JsonObject
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
    public WorkflowBuilder AddThreadVariable(out ThreadVariableReference variable, string name, string? description = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (this._variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        this._variablesMap.Add(name, new JsonObject
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
    public WorkflowBuilder AddUserDefinedVariable(out UserDefinedVariableReference variable, string name, string? description = null, JsonNode? defaultValue = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (this._variablesMap.ContainsKey(name))
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

        this._variablesMap.Add(name, jsonObject);
        variable = new UserDefinedVariableReference(name);
        return this;
    }

    /// <summary>
    /// Adds a state to the workflow.
    /// </summary>
    /// <param name="stateRef">Output parameter that will hold a reference to the state.</param>
    /// <param name="name">The name of the state.</param>
    /// <param name="description">The description of the state.</param>
    /// <param name="configure">Optional builder action to configure the state.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when state name is null, empty, or a state with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddState(out StateReference stateRef, string name, string description, Action<State> configure)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("State name cannot be null or empty", nameof(name));

        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (this._statesMap.ContainsKey(name))
            throw new ArgumentException($"A state with the name '{name}' already exists", nameof(name));

        var state = new State(name, description);
        configure(state);
        this._statesMap.Add(name, state);

        stateRef = new StateReference(name);
        return this;
    }

    /// <summary>
    /// Adds a state to the workflow.
    /// </summary>
    /// <param name="stateRef">Output parameter that will hold a reference to the state.</param>
    /// <param name="state">The state to add.</param>
    /// <exception cref="ArgumentException">Thrown when state is null, or a state with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddState(out StateReference stateRef, State state)
    {
        if (state == null) throw new ArgumentException(nameof(state));

        if (this._statesMap.ContainsKey(state.Name))
            throw new ArgumentException($"A state with the name '{state.Name}' already exists", nameof(state.Name));

        this._statesMap.Add(state.Name, state);
        stateRef = new StateReference(state.Name);
        return this;
    }

    public WorkflowBuilder AddFinalState(out StateReference state, string name = "End", string description = "The final state")
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("State name cannot be null or empty", nameof(name));
        if (this._statesMap.ContainsKey(name))
            throw new ArgumentException($"A state with the name '{name}' already exists", nameof(name));
        this._statesMap.Add(name, new State(name, description).MarkAsFinal());
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

        if (!this._transitionBuilders.TryGetValue(source.Name, out var builder))
        {
            builder = new StateTransitionBuilder(source);
            this._transitionBuilders[source.Name] = builder;
        }
        configure(builder);
        return this;
    }

    public WorkflowBuilder WithStartState(StateReference startState)
    {
        if (startState == null) throw new ArgumentNullException(nameof(startState));
        this._startState = startState.Name;
        return this;
    }

    /// <summary>
    /// Builds the workflow definition.
    /// </summary>
    /// <returns>The completed workflow definition.</returns>
    /// <exception cref="ArgumentNullException">Thrown when startState is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no states are defined or when there are no transitions between states.</exception>
    public JsonObject BuildJson()
    {
        if (this._startState == null)
        {
            throw new InvalidOperationException("Start state must be defined before building the workflow.");
        }

        if (this._statesMap.Count == 0)
        {
            throw new InvalidOperationException("At least one state must be defined.");
        }

        var transitions = new JsonArray();
        foreach (var builder in this._transitionBuilders.Values)
        {
            var transitionsForState = builder.ToJson();
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
        foreach (var variable in this._variablesMap.Values)
        {
            variables.Add(variable);
        }

        var states = new JsonArray();
        foreach (var state in this._statesMap.Values)
        {
            states.Add(state.ToJson());
        }

        var workflow = new JsonObject
        {
            ["name"] = this._name,
            ["variables"] = variables,
            ["states"] = states,
            ["startState"] = this._startState,
            ["transitions"] = transitions
        };

        if (this._description != null)
        {
            workflow["description"] = this._description;
        }

        return workflow;
    }

    public string BuildYaml()
    {
        return new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build()
            .Serialize(this.ToObject(this.BuildJson()));
    }

    /// <summary>
    /// Converts a JsonNode to a regular .NET object structure (Dictionary, List, primitives)
    /// </summary>
    private object? ToObject(JsonNode? node)
    {
        if (node == null)
            return null;

        return node switch
        {
            JsonObject jsonObject => jsonObject.ToDictionary(
                kvp => kvp.Key,
                kvp => ToObject(kvp.Value)),

            JsonArray jsonArray => jsonArray.Select(ToObject).ToList(),

            JsonValue jsonValue => jsonValue.GetValueKind() switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => jsonValue.GetValue<string>(),
                JsonValueKind.Number when jsonValue.TryGetValue<int>(out var intValue) => intValue,
                JsonValueKind.Number when jsonValue.TryGetValue<long>(out var longValue) => longValue,
                JsonValueKind.Number => jsonValue.GetValue<double>(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => jsonValue.ToString()
            },

            _ => node.ToString()
        };
    }
}