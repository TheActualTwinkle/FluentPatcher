

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentPatcher;

/// <summary>
/// JSON converter for <see cref="Patchable{T}"/> that maps a JSON value directly to <c>Patchable.Set(value)</c> or <c>Patchable.NotSet</c>.
/// </summary>
public sealed class PatchableJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Patchable<>);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(PatchableJsonConverter<>).MakeGenericType(innerType);
        
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class PatchableJsonConverter<T> : JsonConverter<Patchable<T>>
    {
        public override Patchable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return Patchable<T>.Set(value);
        }

        public override void Write(Utf8JsonWriter writer, Patchable<T> value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}

