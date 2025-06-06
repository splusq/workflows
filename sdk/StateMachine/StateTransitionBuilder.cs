/// <summary>
/// Builder for defining transitions from a specific source state.
/// </summary>
public sealed class StateTransitionBuilder
{
    private readonly StateReference _source;
    private readonly List<Transition> _transitionsForState = new List<Transition>();

    /// <summary>
    /// Creates a new instance of the <see cref="StateTransitionBuilder"/> class.
    /// </summary>
    /// <param name="source">The source state reference.</param>
    public StateTransitionBuilder(StateReference source)
    {
        this._source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Adds an event-based transition from the source state.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="to">The destination state reference.</param>
    /// <returns>The parent transitions builder.</returns>
    public StateTransitionBuilder OnEventTransitionTo(string eventName, StateReference to)
    {
        if (string.IsNullOrEmpty(eventName)) throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));
        if (to == null) throw new ArgumentNullException(nameof(to));

        this._transitionsForState.Add(new Transition
        {
            From = this._source.Name,
            To = to.Name,
            Event = eventName
        });

        return this;
    }

    /// <summary>
    /// Adds a condition-based transition from the source state.
    /// </summary>
    /// <param name="condition">The condition expression.</param>
    /// <param name="to">The destination state reference.</param>
    /// <returns>The parent transitions builder.</returns>
    public StateTransitionBuilder OnConditionTransitionTo(JmesPathCondition condition, StateReference to)
    {
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        return OnConditionTransitionTo($"jmespath({condition.Expression})", to);
    }

    /// <summary>
    /// Adds a condition-based transition from the source state.
    /// </summary>
    /// <param name="condition">The condition expression.</param>
    /// <param name="to">The destination state reference.</param>
    /// <returns>The parent transitions builder.</returns>
    public StateTransitionBuilder OnConditionTransitionTo(SimpleCondition condition, StateReference to)
    {
        if (condition == null) throw new ArgumentNullException(nameof(condition));
        return OnConditionTransitionTo(condition.Expression, to);
    }

    /// <summary>
    /// Adds a default transition from the source state.
    /// </summary>
    /// <param name="to">The destination state reference.</param>
    /// <returns>The parent transitions builder.</returns>
    public StateTransitionBuilder ByDefaultTransitionTo(StateReference to)
    {
        if (to == null) throw new ArgumentNullException(nameof(to));

        this._transitionsForState.Add(new Transition
        {
            From = this._source.Name,
            To = to.Name
        });

        return this;
    }

    /// <summary>
    /// Builds the transitions for the source state
    /// </summary>
    /// <returns>The transitions for the source state.</returns>
    public List<Transition> Build()
    {
        return this._transitionsForState;
    }

    /// <summary>
    /// Adds a condition-based transition from the source state.
    /// </summary>
    /// <param name="condition">The condition expression.</param>
    /// <param name="to">The destination state reference.</param>
    /// <returns>The parent transitions builder.</returns>
    private StateTransitionBuilder OnConditionTransitionTo(string condition, StateReference to)
    {
        if (string.IsNullOrEmpty(condition)) throw new ArgumentException("Condition expression cannot be null or empty", nameof(condition));
        if (to == null) throw new ArgumentNullException(nameof(to));

        this._transitionsForState.Add(new Transition
        {
            From = this._source.Name,
            To = to.Name,
            Condition = condition
        });

        return this;
    }
}