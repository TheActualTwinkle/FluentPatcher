namespace FluentPatcher.Attributes;

/// <summary>
/// Configures how a property should be patched.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PatchPropertyAttribute : Attribute
{
    /// <summary>
    /// The name of the target property on the entity. If null, uses the same name.
    /// </summary>
    public string? TargetPropertyName { get; set; }
}