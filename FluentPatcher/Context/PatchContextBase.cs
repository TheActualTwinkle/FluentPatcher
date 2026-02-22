namespace FluentPatcher.Context;

/// <summary>
/// Base class for generated patch contexts.
/// </summary>
public abstract class PatchContextBase : IPatchContext
{
    /// <inheritdoc />
    public abstract bool HasChanges();
}