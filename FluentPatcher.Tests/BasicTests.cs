using FluentAssertions;
using FluentPatcher.Tests.Helpers;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for basic patching scenarios.
/// </summary>
public sealed class BasicTests
{
    /// <summary>
    /// Tests that applying a patch to an entity creates a new instance of the entity, ensuring immutability and that the original entity remains unchanged.
    /// </summary>
    [Fact]
    public void ApplyPatch_ShouldCreateNewInstanceOfEntity()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        const string newName = "New Name";

        var patch = new UserUpdateDto
        {
            Name = newName
        };

        // Act.
        var result = patch.ApplyTo(originalUser);

        // Assert.
        result.Entity.Should().NotBeSameAs(originalUser);
    }

    /// <summary>
    /// Tests that when applying a patch that changes a single property, only that property is updated in the resulting entity,
    /// and the change context correctly reflects the change.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenPatchingSingleProperty_ShouldUpdateSingleProperty()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        var newName = originalUser + " Junior";

        var patch = new UserUpdateDto
        {
            Name = newName
        };

        // Act.
        var result = patch.ApplyTo(originalUser);

        // Assert.
        result.Context.NameChanged.Should().BeTrue();
        result.Context.NewName.Should().Be(newName);
        result.Context.OldName.Should().Be(originalUser.Name);

        result.Entity.Should().BeEquivalentTo(originalUser, options => options.Excluding(u => u.Name));

        result.Entity.Name.Should().Be(newName);
    }

    /// <summary>
    /// Tests that when applying a patch that changes multiple properties, all specified properties are updated in the resulting entity,
    /// and the change context correctly reflects all changes.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenPatchingMultipleProperties_ShouldUpdateMultipleProperties()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        var newName = originalUser + " Junior";
        var newAge = originalUser.Age + 5;

        var patch = new UserUpdateDto
        {
            Name = newName,
            Age = newAge
        };

        // Act.
        var result = patch.ApplyTo(originalUser);

        // Assert.
        result.Context.NameChanged.Should().BeTrue();
        result.Context.NewName.Should().Be(newName);
        result.Context.OldName.Should().Be(originalUser.Name);

        result.Context.AgeChanged.Should().BeTrue();
        result.Context.NewAge.Should().Be(newAge);
        result.Context.OldAge.Should().Be(originalUser.Age);

        result.Entity.Should().BeEquivalentTo(originalUser, options => options.Excluding(u => u.Name).Excluding(u => u.Age));

        result.Entity.Name.Should().Be(newName);
        result.Entity.Age.Should().Be(newAge);
    }

    /// <summary>
    /// Tests that when applying a patch that does not change any properties (i.e., all values in the patch are the same as the original entity),
    /// the resulting entity remains unchanged and the change context indicates that no changes were made.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenNoPropertiesChanged_ShouldNotUpdateEntity()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        var patch = new UserUpdateDto
        {
            Name = originalUser.Name,
            Age = originalUser.Age
        };

        // Act.
        var result = patch.ApplyTo(originalUser);

        // Assert.
        result.HasChanges.Should().BeFalse();
        result.Entity.Should().BeEquivalentTo(originalUser);
    }
    
    /// <summary>
    /// Tests that when applying an empty patch (i.e., a patch where no properties are set, a.k.a. <see cref="Patchable.NotSet"/>),
    /// the resulting entity remains unchanged and the change context indicates that no changes were made.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenPatchIsEmpty_ShouldNotUpdateEntity()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        var patch = new UserUpdateDto();

        // Act.
        var result = patch.ApplyTo(originalUser);

        // Assert.
        result.HasChanges.Should().BeFalse();
        
        result.Entity.Should().BeEquivalentTo(originalUser);
    }
}