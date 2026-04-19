using ExternalDomain.Groups;
using FluentAssertions;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Regression tests for generator behavior with external types in generic arguments.
/// </summary>
public sealed class FullyQualifiedTypeGenerationTests
{
    /// <summary>
    /// Ensures generated context and patcher compile and work when entity property types come from another namespace.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenEntityUsesExternalTypeInCollection_ShouldPatchSuccessfully()
    {
        var original = new QualifiedGroupEntity
        {
            AllowedGroupIds = new List<GroupId> { new(new Guid("11111111-1111-1111-1111-111111111111")) }
        };

        var expected = new List<GroupId>
        {
            new(new Guid("22222222-2222-2222-2222-222222222222")),
            new(new Guid("33333333-3333-3333-3333-333333333333"))
        };

        var patch = new QualifiedGroupPatchDto
        {
            AllowedGroupIds = expected
        };

        var result = patch.ApplyTo(original);

        result.Context.AllowedGroupIdsChanged.Should().BeTrue();
        result.Entity.AllowedGroupIds.Should().BeEquivalentTo(expected);
        result.Context.NewAllowedGroupIds.Should().BeEquivalentTo(expected);
    }
}
