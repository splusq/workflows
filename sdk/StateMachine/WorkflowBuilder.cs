using sdk.Common;
using System.Text.Json.Nodes;

/// <summary>
/// Builder for creating workflow definitions using a fluent API.
/// </summary>
public sealed class WorkflowBuilder
{
    private readonly string _name;
    private readonly string? _description;
    private readonly Dictionary<string, Variable> _variablesMap = new Dictionary<string, Variable>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, State> _statesMap = new Dictionary<string, State>();
    private readonly Dictionary<string, List<Transition>> _transitionsForStates = new Dictionary<string, List<Transition>>(StringComparer.InvariantCultureIgnoreCase);
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
    /// <param name="variableRef">Output parameter that will hold a reference to the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="description">The description of the variable.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when variable name is null, empty, or a variable with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddMessagesVariable(out MessagesVariableReference variableRef, string name, string? description = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (this._variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        this._variablesMap.Add(name, new MessagesVariable
        {
            Name = name,
            Description = description
        });
        variableRef = new MessagesVariableReference(name);
        return this;
    }

    /// <summary>
    /// Adds a thread variable to the workflow.
    /// </summary>
    /// <param name="variableRef">Output parameter that will hold a reference to the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="description">The description of the variable.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when variable name is null, empty, or a variable with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddThreadVariable(out ThreadVariableReference variableRef, string name, string? description = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (this._variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        this._variablesMap.Add(name, new ThreadVariable
        {
            Name = name,
            Description = description
        });
        variableRef = new ThreadVariableReference(name);
        return this;
    }

    /// <summary>
    /// Adds a user-defined variable to the workflow.
    /// </summary>
    /// <param name="variableRef">Output parameter that will hold a reference to the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="description">The description of the variable.</param>
    /// <param name="defaultValue">The default value of the variable.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when variable name is null, empty, or a variable with the same name (case-insensitive) already exists.</exception>
    public WorkflowBuilder AddUserDefinedVariable(out UserDefinedVariableReference variableRef, string name, string? description = null, JsonNode? defaultValue = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));

        if (this._variablesMap.ContainsKey(name))
            throw new ArgumentException($"A variable with the name '{name}' already exists", nameof(name));

        var variable = new UserDefinedVariable
        {
            Name = name,
            Description = description,
        };

        if (defaultValue != null)
        {
            variable = variable with
            {
                Value = defaultValue.ToJsonElement()
            };
        }

        this._variablesMap.Add(name, variable);
        variableRef = new UserDefinedVariableReference(name);
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
    public WorkflowBuilder AddState(out StateReference stateRef, string name, string description, Action<StateBuilder> configure)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("State name cannot be null or empty", nameof(name));

        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (this._statesMap.ContainsKey(name))
            throw new ArgumentException($"A state with the name '{name}' already exists", nameof(name));

        var builder = new StateBuilder(name, description);
        configure(builder);
        this._statesMap.Add(name, builder.Build());

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
        this._statesMap.Add(name, new StateBuilder(name, description).MarkAsFinal().Build());
        state = new StateReference(name);
        return this;
    }

    /// <summary>
    /// Adds transitions for a specific source state.
    /// </summary>
    /// <param name="source">The source state reference.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    public WorkflowBuilder AddTransitionsForState(StateReference source, Action<StateTransitionBuilder> configure)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var builder = new StateTransitionBuilder(source);
        configure(builder);
        this._transitionsForStates[source.Name] = builder.Build();
        return this;
    }

    /// <summary>
    /// Adds transitions for a specific source state.
    /// </summary>
    /// <param name="source">The source state reference.</param>
    /// <returns>The updated <see cref="WorkflowBuilder"/> instance.</returns>
    public WorkflowBuilder AddTransitions(List<Transition> transitions)
    {
        if (transitions == null) throw new ArgumentNullException(nameof(transitions));

        foreach (var transition in transitions)
        {
            if (!this._transitionsForStates.TryGetValue(transition.From, out var transitionsForState))
            {
                transitionsForState = new List<Transition>();
                this._transitionsForStates[transition.From] = transitionsForState;
            }
            transitionsForState.Add(transition);
        }

        return this;
    }

    public WorkflowBuilder WithStartState(StateReference startState)
    {
        if (startState == null) throw new ArgumentNullException(nameof(startState));
        this._startState = startState.Name;
        return this;
    }

    public WorkflowDefinition Build()
    {
        return new WorkflowDefinition
        {
            Name = this._name,
            Description = this._description,
            Variables = this._variablesMap.Values.ToList(),
            States = this._statesMap.Values.ToList(),
            StartState = this._startState ?? throw new InvalidOperationException("Start state must be defined before building the workflow."),
            Transitions = this._transitionsForStates.Values.SelectMany(t => t).ToList()
        };
    }
}