

using sdk.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sdk.Agent
{
    public record AgentExtensions
    {
        public required SystemPrompts SystemPrompts { get; init; }
        public required IEnumerable<AgentInputDef> Inputs { get; init; } = new List<AgentInputDef>();
        public required IEnumerable<AgentOutputDef> Outputs { get; init; } = new List<AgentOutputDef>();
        public required IEnumerable<AgentEventDef> Events { get; init; } = new List<AgentEventDef>();

        public AgentExtensionsDefinition ToDef()
        {
            return new AgentExtensionsDefinition
            {
                SystemPrompts = this.SystemPrompts.Overrides != null ? new SystemPromptsDef
                {
                    Overrides = this.SystemPrompts.Overrides
                } : null,
                Inputs = this.Inputs,
                Outputs = this.Outputs,
                Events = this.Events
            };
        }

        public string ToJson() => Serializer.SerializeToJson<AgentExtensions>(this, snakeCase: true);
        public string ToYaml() => Serializer.SerializeToYaml<AgentExtensions>(this, snakeCase: true);
        public static AgentExtensions FromJson(string json) => Serializer.DeserializeFromJson<AgentExtensions>(json, snakeCase: true) ?? throw new InvalidOperationException("Deserialization failed.");
        public static AgentExtensions FromYaml(string yaml) => Serializer.DeserializeFromYaml<AgentExtensions>(yaml, snakeCase: true) ?? throw new InvalidOperationException("Deserialization failed.");
    }

    public record AgentExtensionsDefinition
    {
        public SystemPromptsDef? SystemPrompts { get; init; }
        public IEnumerable<AgentInputDef>? Inputs { get; init; } = new List<AgentInputDef>();
        public IEnumerable<AgentOutputDef>? Outputs { get; init; } = new List<AgentOutputDef>();
        public IEnumerable<AgentEventDef>? Events { get; init; } = new List<AgentEventDef>();

        public string ToJson() => Serializer.SerializeToJson<AgentExtensionsDefinition>(this, snakeCase: true);
        public string ToYaml() => Serializer.SerializeToYaml<AgentExtensionsDefinition>(this, snakeCase: true);
        public static AgentExtensionsDefinition FromJson(string json) => Serializer.DeserializeFromJson<AgentExtensionsDefinition>(json, snakeCase: true) ?? throw new InvalidOperationException("Deserialization failed.");
        public static AgentExtensionsDefinition FromYaml(string yaml) => Serializer.DeserializeFromYaml<AgentExtensionsDefinition>(yaml, snakeCase: true) ?? throw new InvalidOperationException("Deserialization failed.");
    }

    public record SystemPromptsDef
    {
        public required SystemPromptDetails Overrides { get; init; }
    }

    public record SystemPrompts
    {
        public SystemPromptDetails? Overrides { get; init; }
        public SystemPromptDetails? Defaults { get; init; }
    }

    public record SystemPromptDetails
    {
        public string? Context { get; init; }
        public string? Events { get; init; }
        public string? Outputs { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(GlobalInputDef), typeDiscriminator: "global")]
    [JsonDerivedType(typeof(LlmInputDef), typeDiscriminator: "llm")]
    [JsonDerivedType(typeof(ToolsInputDef), typeDiscriminator: "tools")]
    public record AgentInputDef
    {
        internal AgentInputDef() { }

        public required string Name { get; init; }

        public required string Description { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public JsonElement Default { get; set; }
    }

    public record GlobalInputDef : AgentInputDef
    {
        public IEnumerable<string> ExplicitBindings { get; init; } = new List<string>();
    }

    public record ToolsInputDef : AgentInputDef
    {
        public IEnumerable<string> ExplicitBindings { get; init; } = new List<string>();
    }

    public record LlmInputDef : AgentInputDef
    {
    }


    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(LlmOutputDef), typeDiscriminator: "llm")]
    [JsonDerivedType(typeof(EvalOutputDef), typeDiscriminator: "eval")]
    public record AgentOutputDef
    {
        internal AgentOutputDef() { }
        public required string Name { get; init; }

        public required string Description { get; init; }
    }

    public record LlmOutputDef : AgentOutputDef
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public JsonElement Schema { get; init; }
    }

    public record EvalOutputDef : AgentOutputDef
    {
        public required string Expression { get; init; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(LlmEventDef), typeDiscriminator: "llm")]
    [JsonDerivedType(typeof(EvalEventDef), typeDiscriminator: "eval")]
    public record AgentEventDef
    {
        internal AgentEventDef() { }
        public required string Name { get; init; }
        public required string Condition { get; init; }
        public required string Description { get; init; }
    }

    public record LlmEventDef : AgentEventDef
    {
    }

    public record EvalEventDef : AgentEventDef
    {
    }

    public record RunOutputs
    {
        public IEnumerable<RunOutput> Data { get; init; } = new List<RunOutput>();

        public static RunOutputs FromJson(string json) => Serializer.DeserializeFromJson<RunOutputs>(json, snakeCase: true) ?? throw new InvalidOperationException("Deserialization failed.");
    }

    public record RunOutput
    {
        public required string Name { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required JsonElement Value { get; init; }
    }

    public record RunEvents
    {
        public IEnumerable<RunEvent> Data { get; init; } = new List<RunEvent>();

        public static RunEvents FromJson(string json) => Serializer.DeserializeFromJson<RunEvents>(json, snakeCase: true) ?? throw new InvalidOperationException("Deserialization failed.");
    }

    public record RunEvent
    {
        public required string Name { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
    }
}
