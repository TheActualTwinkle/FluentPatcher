using FluentPatcher.Attributes;

namespace FluentPatcher.Tests.Models;

/// <summary>
/// Entity class, used to test patching behavior when property names differ between the patch request and the entity.
/// </summary>
public sealed class RenamedEntity
{
    /// <summary>
    /// Just some name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Patch request class with a property name that differs from the target entity, and without using the <see cref="PatchPropertyAttribute"/> to map it.
/// </summary>
[PatchFor(typeof(RenamedEntity))]
public sealed class RenamedEntityPatchWithoutAttribute
{ 
    /// <summary>
    /// This property is intended to update the <see cref="RenamedEntity.Name"/> property,
    /// but since it has a different name and lacks the <see cref="PatchPropertyAttribute"/>, it will not be mapped to the entity's property during patching.
    /// </summary>
#pragma warning disable FP0002
    public Patchable<string?> DifferentName { get; init; }
#pragma warning restore FP0002
}

/// <summary>
/// Patch request class with a property name that differs from the target entity, but using the <see cref="PatchPropertyAttribute"/> to explicitly map it to the correct entity property.
/// </summary>
[PatchFor(typeof(RenamedEntity))]
public sealed class RenamedEntityPatchWithAttribute
{
    /// <summary>
    /// This property is intended to update the <see cref="RenamedEntity.Name"/> property.
    /// The <see cref="PatchPropertyAttribute"/> is used to specify that this property should be mapped to the <see cref="RenamedEntity.Name"/> property during patching, despite the difference in names.
    /// </summary>
    [PatchProperty(TargetPropertyName = nameof(RenamedEntity.Name))]
    public Patchable<string?> DifferentName { get; init; }
}

