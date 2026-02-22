using FluentPatcher.Attributes;

namespace FluentPatcher.Tests.Models;

/// <summary>
/// Entity used to test collection patch strategies. Each collection property is patched by a dedicated patch class
/// with a specific <see cref="CollectionPatchStrategyAttribute"/> configuration.
/// </summary>
public sealed class CollectionStrategyEntity
{
    /// <summary>
    /// Collection for testing <see cref="CollectionPatchStrategy.Replace"/>.
    /// </summary>
    public List<Address> ReplaceItems { get; set; } = [];

    // /// <summary>
    // /// Collection for testing <see cref="CollectionPatchStrategy.Append"/> behavior.
    // /// </summary>
    // public List<Address> AppendItems { get; set; } = [];
}

/// <summary>
/// Patch class for <see cref="CollectionStrategyEntity"/> that targets <see cref="CollectionStrategyEntity.ReplaceItems"/>
/// and uses the <see cref="CollectionPatchStrategy.Replace"/> strategy.
/// </summary>
[PatchFor(typeof(CollectionStrategyEntity))]
public sealed class CollectionStrategyReplaceUpdateDto
{
    /// <summary>
    /// Collection to replace the existing one on the entity. When applied, the entire collection on the entity will be replaced with this new collection.
    /// </summary>
    [CollectionPatchStrategy(Strategy = CollectionPatchStrategy.Replace)]
    public Patchable<List<Address>?> ReplaceItems { get; init; }
}

// /// <summary>
// /// Patch class for <see cref="CollectionStrategyEntity"/> that targets <see cref="CollectionStrategyEntity.AppendItems"/>
// /// and uses <see cref="CollectionPatchStrategy.Append"/> strategy.
// /// </summary>
// [PatchFor(typeof(CollectionStrategyEntity))]
// public sealed class CollectionStrategyAppendPatch
// {
//     [CollectionPatchStrategy(Strategy = CollectionPatchStrategy.Append)]
//     public Patchable<List<Address>> AppendItems { get; init; }
// }
