using System.Text.Json.Nodes;

/// <summary>
/// Builder for defining actions that are executed when entering an agent actor.
/// </summary>
public sealed class AgentActorOnEnterActionsBuilder
{
    private string _threadResetMode = "Never";

    /// <summary>
    /// Adds an action to reset the thread.
    /// </summary>
    /// <returns>The updated <see cref="AgentActorOnEnterActionsBuilder"/> instance.</returns>
    public AgentActorOnEnterActionsBuilder ResetThread()
    {
        this._threadResetMode = "OnEnter";
        return this;
    }

    /// <summary>
    /// Builds the agent actor.
    /// </summary>
    /// <returns>The agent actor.</returns>
    internal JsonObject BuildJsontWith(JsonObject actor)
    {
        actor["threadResetMode"] = this._threadResetMode;
        return actor;
    }
}