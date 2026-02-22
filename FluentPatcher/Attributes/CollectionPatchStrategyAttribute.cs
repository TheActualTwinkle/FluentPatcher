namespace FluentPatcher.Attributes;

/// <summary>
/// Specifies how a collection property should be patched.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CollectionPatchStrategyAttribute : Attribute
{
    /// <summary>
    /// The strategy to use when patching the collection.
    /// </summary>
    /// <remarks>
    /// The default strategy is <see cref="CollectionPatchStrategy.Replace"/>.
    /// </remarks>
    public CollectionPatchStrategy Strategy { get; set; } = CollectionPatchStrategy.Replace;
}

/// <summary>
/// Defines how collection properties should be patched.
/// </summary>
public enum CollectionPatchStrategy
{
    /// <summary>
    /// Replace the entire collection with the new one.
    /// </summary>
    Replace,

    // /// <summary>
    // /// Add new items to the existing collection without removing any.
    // /// </summary>
    // Append
}