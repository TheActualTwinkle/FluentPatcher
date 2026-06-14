namespace FluentPatcher;

/// <summary>
/// Defines how collection values are compared when detecting patch changes.
/// </summary>
public enum CollectionChangeComparison
{
    /// <summary>
    /// Compare collection values with the default object equality behavior.
    /// </summary>
    Reference,

    /// <summary>
    /// Compare collection values item by item in enumeration order.
    /// </summary>
    Sequence
}

/// <summary>
/// Options that control patch application behavior.
/// </summary>
public sealed class PatchOptions
{
    /// <summary>
    /// Gets or sets the global default collection comparison mode.
    /// </summary>
    public static CollectionChangeComparison DefaultCollectionComparison { get; set; } = CollectionChangeComparison.Sequence;

    /// <summary>
    /// Gets or sets the collection comparison mode for a single patch call.
    /// When null, <see cref="DefaultCollectionComparison"/> is used.
    /// </summary>
    public CollectionChangeComparison? CollectionComparison { get; set; }
}
