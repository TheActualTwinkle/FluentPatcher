using FluentAssertions;
using FluentPatcher.Attributes;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for applying patches to entities where the property names in the patch differ from the property names in the entity,
/// and how the presence or absence of the <see cref="PatchPropertyAttribute"/> affects the patching behavior and the context of changes.
/// </summary>
public sealed class RenamedPropertyPatchTests
{
    /// <summary>
    /// Tests that when applying a patch where the property name differs from the entity's property and no <see cref="PatchPropertyAttribute"/> is present,
    /// the entity's property should not be updated.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenPropertyNameDiffersAndNoPatchPropertyAttribute_ShouldNotUpdateEntityProperty()
    {
        // Arrange.
        var entity = new RenamedEntity { Name = "Original" };

        var patch = new RenamedEntityPatchWithoutAttribute
        {
            DifferentName = "New Name"
        };

        // Act.
        var result = patch.ApplyTo(entity);

        // Assert.
        result.HasChanges.Should().BeFalse();
    }

    /// <summary>
    /// Tests that when applying a patch where the property name differs from the entity's property but a <see cref="PatchPropertyAttribute"/> is present,
    /// the entity's property should be updated and the context should reflect the change.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenPropertyNameDiffersButPatchPropertyAttributePresent_ShouldUpdateMappedEntityProperty()
    {
        // Arrange.
        var entity = new RenamedEntity { Name = "Original" };

        const string newName = "New Name";

        var patch = new RenamedEntityPatchWithAttribute
        {
            DifferentName = newName
        };

        // Act.
        var result = patch.ApplyTo(entity);

        // Assert.
        result.HasChanges.Should().BeTrue();

        result.Context.DifferentNameChanged.Should().BeTrue();
        
        result.Entity.Name.Should().Be(newName);
    }
}