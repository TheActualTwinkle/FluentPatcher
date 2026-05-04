using FluentPatcher.Attributes;

namespace FluentPatcher.Tests.Models;

/// <summary>
/// Entity with a non-public parameterless constructor to validate cloning support.
/// </summary>
public sealed class PrivateCtorEntity
{
    /// <summary>
    /// Gets the current name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    private PrivateCtorEntity()
    {
    }

    /// <summary>
    /// Creates a new instance with the provided name.
    /// </summary>
    public PrivateCtorEntity(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Patch request for <see cref="PrivateCtorEntity"/>.
/// </summary>
[PatchFor(typeof(PrivateCtorEntity))]
public sealed class PrivateCtorUpdateDto
{
    /// <summary>
    /// New name for the entity.
    /// </summary>
    public Patchable<string> Name { get; init; }
}
