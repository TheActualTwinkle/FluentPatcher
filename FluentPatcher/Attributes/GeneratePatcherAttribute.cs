namespace FluentPatcher.Attributes;

/// <summary>
/// Marks a class for source generation of an entity patcher.
/// The generator will create a Patcher class and PatchContext for tracking changes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PatchForAttribute : Attribute
{
    /// <summary>
    /// Creates a new attribute that binds this DTO to a specific target entity type.
    /// </summary>
    /// <param name="targetEntityType">The entity type this DTO patches.</param>
    public PatchForAttribute(Type targetEntityType) =>
        TargetEntityType = targetEntityType ?? throw new ArgumentNullException(nameof(targetEntityType));

    /// <summary>
    /// The target entity type to patch.
    /// </summary>
    public Type TargetEntityType { get; }
}