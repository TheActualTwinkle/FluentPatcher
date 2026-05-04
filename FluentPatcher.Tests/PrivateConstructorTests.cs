using FluentAssertions;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for patching entities with non-public parameterless constructors.
/// </summary>
public sealed class PrivateConstructorTests
{
    /// <summary>
    /// Ensures patches can be applied when only a non-public parameterless constructor exists.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenEntityHasPrivateParameterlessCtor_ShouldCloneAndPatch()
    {
        // Arrange.
        var original = new PrivateCtorEntity("Original");
        var patch = new PrivateCtorUpdateDto
        {
            Name = "Updated"
        };

        // Act.
        var result = patch.ApplyTo(original);

        // Assert.
        result.Entity.Should().NotBeSameAs(original);
        result.Entity.Name.Should().Be("Updated");
        result.Context.NameChanged.Should().BeTrue();
    }
}
