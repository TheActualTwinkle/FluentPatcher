using FluentPatcher.Attributes;

namespace FluentPatcher.Tests.Models;

/// <summary>
/// Entity with an internal property used to verify patching supports non-public members.
/// </summary>
public class InternalEmailEntity
{
    /// <summary>
    /// A regular public property.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Internal property should still be patchable.
    /// </summary>
    internal string? Email { get; set; }
}

/// <summary>
/// Patch DTO for <see cref="InternalEmailEntity"/>.
/// </summary>
[PatchFor(typeof(InternalEmailEntity))]
public class InternalEmailUpdateDto
{
    /// <summary>
    /// Patchable email; should patch <see cref="InternalEmailEntity.Email"/>.
    /// </summary>
    public Patchable<string?> Email { get; init; }
}
