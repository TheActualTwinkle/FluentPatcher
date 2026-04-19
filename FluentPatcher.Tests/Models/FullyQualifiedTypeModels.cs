using ExternalDomain.Groups;
using FluentPatcher.Attributes;

namespace ExternalDomain.Groups
{
    /// <summary>
    /// External identifier type used to verify generator output with fully-qualified type names.
    /// </summary>
    public readonly record struct GroupId(Guid Value);
}

namespace FluentPatcher.Tests.Models
{
    /// <summary>
    /// Entity with an internal collection property that uses an external item type.
    /// </summary>
    public sealed class QualifiedGroupEntity
    {
        /// <summary>
        /// Allowed groups.
        /// </summary>
        internal ICollection<GroupId> AllowedGroupIds { get; set; } = [];
    }

    /// <summary>
    /// Patch DTO for <see cref="QualifiedGroupEntity"/>.
    /// </summary>
    [PatchFor(typeof(QualifiedGroupEntity))]
    public sealed class QualifiedGroupPatchDto
    {
        /// <summary>
        /// New allowed groups.
        /// </summary>
        public Patchable<ICollection<GroupId>> AllowedGroupIds { get; init; }
    }
}
