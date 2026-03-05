using FluentAssertions;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests patching behavior against internal properties on the target entity.
/// </summary>
public sealed class InternalPropertyPatchTests
{
    /// <summary>
    /// Ensures an internal target property is updated.
    /// </summary>
    [Fact]
    public void ApplyTo_WhenTargetPropertyIsInternal_ShouldPatchInternalProperty()
    {
        // Arrange.
        const string name = "A";
        const string oldEmail = "old@example.com";
        
        var entity = new InternalEmailEntity { Name = name, Email = oldEmail };

        const string newEmail = "new@example.com";

        var patch = new InternalEmailUpdateDto
        {
            Email = newEmail
        };

        // Act.
        var result = patch.ApplyTo(entity);
        
        // Assert.
        result.HasChanges.Should().BeTrue();
        result.Context.EmailChanged.Should().BeTrue();
        result.Entity.Email.Should().Be(newEmail);
    }
}
