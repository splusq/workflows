using sdk.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UserDefinedVariable), typeDiscriminator: "userDefined")]
[JsonDerivedType(typeof(ThreadVariable), typeDiscriminator: "thread")]
[JsonDerivedType(typeof(MessagesVariable), typeDiscriminator: "messages")]
public record Variable
{
    [JsonConstructor]
    internal Variable() { }
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record UserDefinedVariable : Variable
{
    [JsonConstructor]
    internal UserDefinedVariable() : base()
    {
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public JsonElement Value { get; init; }
}

public record MessagesVariable : Variable
{
    [JsonConstructor]
    internal MessagesVariable() : base()
    {
    }
}

public record ThreadVariable : Variable
{
    [JsonConstructor]
    internal ThreadVariable() : base()
    {
    }
}

public enum ThreadResetMode
{
    Never = 0,
    OnEnter
}

public enum HumanInLoopMode
{
    /// <summary>
    /// Waits for a human to provide input on the chat if no new messages are available for the agent to operate on.
    /// </summary>
    OnNoMessage = 0,

    /// <summary>
    /// Never waits for human input, the agent will continue processing without human intervention.
    /// </summary>
    Never,

    /// <summary>
    /// Always waits for human input before proceeding with the next action.
    /// </summary>
    Always
}

public record AgentActor
{
    [JsonConstructor]
    internal AgentActor() { }

    public required string Id { get; init; }
    public string? Agent { get; init; }
    public string? AgentId { get; init; }
    public string? Thread { get; init; }
    public ThreadResetMode ThreadResetMode { get; init; } = ThreadResetMode.Never;
    public IEnumerable<string>? MessagesIn { get; init; }
    public string? MessagesOut { get; init; }
    public string? UserMessages { get; init; }
    public Dictionary<string, string>? Inputs { get; init; }
    public Dictionary<string, string>? Outputs { get; init; }
    public Dictionary<string, string>? Events { get; init; }
    public HumanInLoopMode HumanInLoopMode { get; init; } = HumanInLoopMode.OnNoMessage;
    public bool StreamOutput { get; init; } = true;
    public int? MaxTurn { get; init; }
    public int? MaxTransientErrorRetries { get; init; }
    public int? MaxRateLimitRetries { get; init; }
}

public record State
{
    [JsonConstructor]
    internal State() { }

    public required string Name { get; init; }
    public string? Description { get; init; }
    public IEnumerable<AgentActor>? Actors { get; init; }
    public bool IsFinal { get; init; }
}

public record Transition
{
    [JsonConstructor]
    internal Transition() { }

    public required string From { get; init; }
    public required string To { get; init; }
    public string? Condition { get; init; }
    public string? Event { get; init; }
}

public record WorkflowDefinition
{
    [JsonConstructor]
    internal WorkflowDefinition() { }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public IEnumerable<Variable> Variables { get; init; } = new List<Variable>();

    public IEnumerable<State> States { get; init; } = new List<State>();

    public required string StartState { get; init; }

    public IEnumerable<Transition> Transitions { get; init; } = new List<Transition>();

    public string ToJson() => Serializer.SerializeToJson<WorkflowDefinition>(this);
    public string ToYaml() => Serializer.SerializeToYaml<WorkflowDefinition>(this);
    public static WorkflowDefinition FromJson(string json) => Serializer.DeserializeFromJson<WorkflowDefinition>(json) ?? throw new InvalidOperationException("Deserialization failed.");
    public static WorkflowDefinition FromYaml(string yaml) => Serializer.DeserializeFromYaml<WorkflowDefinition>(yaml) ?? throw new InvalidOperationException("Deserialization failed.");
}

public record Workflow : WorkflowDefinition
{
    [JsonConstructor]
    internal Workflow() : base() { }

    public required string Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }

    public new string ToJson() => Serializer.SerializeToJson<Workflow>(this);
    public new string ToYaml() => Serializer.SerializeToYaml<Workflow>(this);
    public static new Workflow FromJson(string json) => Serializer.DeserializeFromJson<Workflow>(json) ?? throw new InvalidOperationException("Deserialization failed.");
    public static new Workflow FromYaml(string yaml) => Serializer.DeserializeFromYaml<Workflow>(yaml) ?? throw new InvalidOperationException("Deserialization failed.");
}