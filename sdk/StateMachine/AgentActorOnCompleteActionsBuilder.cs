public enum AgentActorMessageSource
{
    OutputMessages = 0,
    UserMessages = 1
}

public enum AgentActorUserDefinedSource
{
    Outputs = 0,
    Events
}

/// <summary>
/// Builder for defining actions that are executed when entering or exiting a state.
/// </summary>
public sealed class AgentActorOnCompleteActionsBuilder
{
    private readonly Dictionary<string, string> _outputs = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    private readonly Dictionary<string, string> _events = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    private string? _messagesOut;
    private string? _userMessages;

    /// <summary>
    /// Adds an action to set a messages variable.
    /// </summary>
    /// <param name="variable">The variable reference to set.</param>
    /// <param name="source">The source from which to get the variable value from.</param>
    /// <returns>The updated <see cref="AgentActorOnCompleteActionsBuilder"/> instance.</returns>
    public AgentActorOnCompleteActionsBuilder SetMessagesVariable(MessagesVariableReference variable, AgentActorMessageSource source)
    {
        if (variable == null) throw new ArgumentNullException(nameof(variable));

        switch (source)
        {
            case AgentActorMessageSource.OutputMessages:
                if (this._messagesOut != null)
                {
                    throw new InvalidOperationException("Setting multiple messages variables with OutputMessages is not supported at this time.");
                }

                this._messagesOut = variable.Name;
                break;
            case AgentActorMessageSource.UserMessages:
                if (this._messagesOut != null)
                {
                    throw new InvalidOperationException("Setting multiple messages variables with UserMessages is not supported at this time.");
                }

                this._userMessages = variable.Name;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }
        return this;
    }

    /// <summary>
    /// Adds an action to set a user-defined variable.
    /// </summary>
    /// <param name="variable">The variable reference to set.</param>
    /// <param name="source">The source from which to get the variable value from.</param>
    /// <param name="name">The name of the agent output or event.</param>
    /// <returns>The updated <see cref="AgentActorOnCompleteActionsBuilder"/> instance.</returns>
    public AgentActorOnCompleteActionsBuilder SetUserDefinedVariable(UserDefinedVariableReference variable, AgentActorUserDefinedSource source, string name)
    {
        if (variable == null) throw new ArgumentNullException(nameof(variable));
            
        switch (source)
        {
            case AgentActorUserDefinedSource.Outputs:
                if (this._outputs.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Setting multiple user-defined variables from the same Output '{name}' is not supported at this time.");
                }
                this._outputs[name] = variable.Name;
                break;
            case AgentActorUserDefinedSource.Events:
                if (this._events.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Setting multiple user-defined variables from the same Event '{name}' is not supported at this time.");
                }
                this._events[name] = variable.Name;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(source), source, null);
        }

        return this;
    }

    /// <summary>
    /// Builds the agent actor.
    /// </summary>
    /// <returns>The agent actor.</returns>
    public AgentActor Build(AgentActor actor)
    {
        return actor with
        {
            MessagesOut = this._messagesOut,
            UserMessages = this._userMessages,
            Outputs = this._outputs.Any() ? this._outputs : null,
            Events = this._events.Any() ? this._events : null
        };
    }
}