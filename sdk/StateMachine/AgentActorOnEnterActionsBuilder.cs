/// <summary>
/// Builder for defining actions that are executed when entering an agent actor.
/// </summary>
public sealed class AgentActorOnEnterActionsBuilder
{
    private ThreadResetMode _threadResetMode = ThreadResetMode.Never;

    /// <summary>
    /// Adds an action to reset the thread.
    /// </summary>
    /// <returns>The updated <see cref="AgentActorOnEnterActionsBuilder"/> instance.</returns>
    public AgentActorOnEnterActionsBuilder ResetThread()
    {
        this._threadResetMode = ThreadResetMode.OnEnter;
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
            ThreadResetMode = this._threadResetMode
        };
    }
}