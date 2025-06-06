using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace sdk.Common
{
    public static class Serializer
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = BuildJsonSerializerOptions(JsonNamingPolicy.CamelCase);
        private static readonly JsonSerializerOptions jsonSerializerOptionsSnakeCase = BuildJsonSerializerOptions(JsonNamingPolicy.SnakeCaseLower);
        private static readonly ISerializer yamlSerializer = 
            new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();
        private static readonly IDeserializer yamlDeSerializer = 
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

        private static JsonSerializerOptions BuildJsonSerializerOptions(JsonNamingPolicy jsonNamingPolicy)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = jsonNamingPolicy,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            options.Converters.Add(new JsonStringEnumConverter(jsonNamingPolicy));
            options.Converters.Add(new UnixTimeToDateTimOffsetConverter());
            return options;
        }

        public static string SerializeToJson<T>(T value, bool snakeCase = false)
        {
            return JsonSerializer.Serialize(value, snakeCase ? jsonSerializerOptionsSnakeCase : jsonSerializerOptions);
        }

        public static TValue? DeserializeFromJson<TValue>(string json, bool snakeCase = false)
        {
            return JsonSerializer.Deserialize<TValue>(json, snakeCase ? jsonSerializerOptionsSnakeCase : jsonSerializerOptions);
        }

        public static TValue? DeserializeFromYaml<TValue>(string yaml, bool snakeCase = false)
        {
            return JsonSerializer.Deserialize<TValue>(ConvertYamlToJson(yaml), snakeCase ? jsonSerializerOptionsSnakeCase : jsonSerializerOptions);
        }

        public static string SerializeToYaml<T>(T value, bool snakeCase = false)
        {
            var json = SerializeToJson(value, snakeCase);
            return ConvertJsonToYaml(json);
        }

        public static string ConvertJsonToYaml(string json)
        {
            return yamlSerializer.Serialize(ToObject(JsonNode.Parse(json)));
        }

        public static string ConvertYamlToJson(string yaml)
        {
            var deserializedObject = yamlDeSerializer.Deserialize(yaml);
            return SerializeToJson(deserializedObject);
        }

        public static JsonElement ToJsonElement(this JsonNode? json)
        {
            if (json is null)
            {
                using JsonDocument doc = JsonDocument.Parse("null");
                return doc.RootElement.Clone();
            }

            using (JsonDocument doc = JsonDocument.Parse(json.ToJsonString()))
            {
                return doc.RootElement.Clone();
            }
        }

        public static JsonNode? ToJsonNode(this JsonElement json)
        {
            switch (json.ValueKind)
            {
                case JsonValueKind.Array:
                    return JsonNode.Parse(json.GetRawText());
                case JsonValueKind.Object:
                    return JsonNode.Parse(json.GetRawText());
                case JsonValueKind.String:
                    return JsonValue.Create<string>(json.GetString());
                case JsonValueKind.Number:
                    return JsonValue.Create(json.GetDouble());
                case JsonValueKind.True:
                    return JsonValue.Create(true);
                case JsonValueKind.False:
                    return JsonValue.Create(false);
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return JsonValue.Create<string>(null);
                default:
                    return JsonNode.Parse(json.GetRawText());
            }
        }

        private static object? ToObject(JsonNode? node)
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

        private class UnixTimeToDateTimOffsetConverter : JsonConverter<DateTimeOffset>
        {
            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    if (reader.TryGetInt64(out long time))
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(time);
                    }
                }
                catch
                {
                    // TryGetInt64 still can throw exceptions if invalid (e.g. no number)

                }

                return DateTimeOffset.MinValue;
            }

            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value.ToUnixTimeSeconds());
            }
        }
    }
}
