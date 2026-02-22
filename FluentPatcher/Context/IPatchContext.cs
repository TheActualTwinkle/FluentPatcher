namespace FluentPatcher.Context;

/// <summary>
/// Interface for patch contexts that track changes during patching.
/// </summary>
public interface IPatchContext
{
    /// <summary>
    /// Returns true if any property was changed during patching.
    /// </summary>
    bool HasChanges();
}