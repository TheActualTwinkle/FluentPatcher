using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for JSON (de)serialization of <see cref="Patchable{T}"/> when using the
/// <see cref="PatchableJsonConverterFactory"/>. Covers scenarios for present properties, null values,
/// missing properties and serialization of set values.
/// </summary>
public sealed class PatchableJsonConverterTests
{
    /// <summary>
    /// Creates <see cref="JsonSerializerOptions"/> with the <see cref="PatchableJsonConverterFactory"/> registered.
    /// </summary>
    private static JsonSerializerOptions CreateOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new PatchableJsonConverterFactory() }
        };

    private sealed class Model
    {
        public Patchable<string?> Name { get; set; }

        public Patchable<int> Age { get; set; }
    }

    /// <summary>
    /// Deserializing a JSON payload that contains the property should mark the corresponding
    /// <see cref="Patchable{T}"/> as set (HasValue = true) and populate the underlying value.
    /// </summary>
    [Fact]
    public void Deserialize_WhenJsonContainsProperty_ShouldSetHasValueAndPopulateValue()
    {
        var json = "{\"name\":\"Alice\"}";

        var result = JsonSerializer.Deserialize<Model>(json, CreateOptions());

        result.Should().NotBeNull();
        result.Name.HasValue.Should().BeTrue();
        result.Name.Value.Should().Be("Alice");
    }

    /// <summary>
    /// Deserializing a JSON payload where the property value is null should mark the
    /// <see cref="Patchable{T}"/> as set (HasValue = true) and set the underlying value to null
    /// (not leave it as NotSet).
    /// </summary>
    [Fact]
    public void Deserialize_WhenJsonContainsNull_ShouldSetHasValueAndValueNull()
    {
        var json = "{\"name\":null}";

        var result = JsonSerializer.Deserialize<Model>(json, CreateOptions());

        result.Should().NotBeNull();
        result.Name.HasValue.Should().BeTrue();
        result.Name.Value.Should().BeNull();
    }

    /// <summary>
    /// When the JSON payload does not contain the property, the corresponding
    /// <see cref="Patchable{T}"/> should remain NotSet (HasValue = false).
    /// </summary>
    [Fact]
    public void Deserialize_WhenPropertyMissing_ShouldLeavePatchableNotSet()
    {
        var json = "{}";

        var result = JsonSerializer.Deserialize<Model>(json, CreateOptions());

        result.Should().NotBeNull();
        result.Name.HasValue.Should().BeFalse();
    }

    /// <summary>
    /// Serializing a model where a <see cref="Patchable{T}"/> has been explicitly set should
    /// write the underlying value into the JSON payload.
    /// </summary>
    [Fact]
    public void Serialize_WhenPatchableSet_ShouldWriteUnderlyingValue()
    {
        var model = new Model { Age = Patchable<int>.Set(42) };

        var json = JsonSerializer.Serialize(model, CreateOptions());

        json.Should().Contain("\"Age\":42");
    }
}