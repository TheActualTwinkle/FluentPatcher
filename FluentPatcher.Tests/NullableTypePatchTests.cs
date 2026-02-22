using FluentAssertions;
using FluentPatcher.Tests.Helpers;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for patching nullable type properties.
/// Ensuring that changes from <c>null</c> to a value and from a value to <c>null</c> are correctly handled and detected in the context.
/// </summary>
public sealed class NullableTypePatchTests
{
    /// <summary>
    /// Ensures that if we apply a patch that sets a nullable reference type property from <c>null</c> to a new value,
    /// the property is updated to the new value and the change is correctly detected in the context.
    /// </summary>
    [Fact]
    public void ApplyPatchFromNullToValue_WhenPatchingNullableReferenceTypeProperty_ShouldUpdatePropertyToNewValue()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        originalUser.Email = null;

        const string newEmail = "test@lol.com";
        
        var patch = new UserUpdateDto
        {
            Email = newEmail
        };
        
        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.Context.EmailChanged.Should().BeTrue();
        result.Context.NewEmail.Should().Be(newEmail);
        result.Context.OldEmail.Should().BeNull();
    }
    
    /// <summary>
    /// Ensures that if we apply a patch that sets a nullable reference type property to <c>null</c>,
    /// the property is updated to <c>null</c> and the change is correctly detected in the context.
    /// </summary>
    [Fact]
    public void ApplyPatchFromValueToNull_WhenPatchingNullableReferenceTypeProperty_ShouldUpdatePropertyToNull()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        var patch = new UserUpdateDto
        {
            Email = null
        };
        
        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.Context.EmailChanged.Should().BeTrue();
        result.Context.NewEmail.Should().BeNull();
        result.Context.OldEmail.Should().Be(originalUser.Email);
    }

    /// <summary>
    /// Ensures that if we apply an empty patch (i.e., a patch that doesn't specify any changes) to a nullable reference type property,
    /// the original value remains unchanged and no changes are detected.
    /// </summary>
    /// <param name="originalEmail">The original email value, that must persist after applying the empty patch.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("test@lol.com")]
    public void ApplyEmptyPatch_WhenPatchingNullableReferenceTypeProperty_ShouldNotUpdateProperty(string? originalEmail)
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        originalUser.Email = originalEmail;

        var patch = new UserUpdateDto();
        
        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.Context.EmailChanged.Should().BeFalse();
        result.Context.NewEmail.Should().Be(originalUser.Email);
        result.Context.OldEmail.Should().Be(originalUser.Email);
    }
    
    /// <summary>
    /// Ensures that if we apply a patch that sets a nullable value type property from <c>null</c> to a new value,
    /// the property is updated to the new value and the change is correctly detected in the context.
    /// </summary>
    [Fact]
    public void ApplyPatchFromNullToValue_WhenPatchingNullableValueTypeProperty_ShouldUpdatePropertyToNewValue()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        originalUser.Age = null;

        const int newAge = 30;
        
        var patch = new UserUpdateDto
        {
            Age = newAge
        };
        
        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.Context.AgeChanged.Should().BeTrue();
        result.Context.NewAge.Should().Be(newAge);
        result.Context.OldAge.Should().BeNull();
    }
    
    /// <summary>
    /// Ensures that if we apply a patch that sets a nullable value type property to <c>null</c>,
    /// the property is updated to <c>null</c> and the change is correctly detected in the context.
    /// </summary>
    [Fact]
    public void ApplyPatchFromValueToNull_WhenPatchingNullableValueTypeProperty_ShouldUpdatePropertyToNull()
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        var patch = new UserUpdateDto
        {
            Age = null
        };
        
        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.Context.AgeChanged.Should().BeTrue();
        result.Context.NewAge.Should().BeNull();
        result.Context.OldAge.Should().Be(originalUser.Age);
    }

    /// <summary>
    /// Ensures that if we apply an empty patch (i.e., a patch that doesn't specify any changes) to a nullable value type property,
    /// the original value remains unchanged and no changes are detected.
    /// </summary>
    /// <param name="originalAge">The original age value, that must persist after applying the empty patch.</param>
    [Theory]
    [InlineData(null)]
    [InlineData(18)]
    public void ApplyEmptyPatch_WhenPatchingNullableValueTypeProperty_ShouldNotUpdateProperty(int? originalAge)
    {
        // Arrange.
        var originalUser = UserHelper.CreateUser();

        originalUser.Age = originalAge;

        var patch = new UserUpdateDto();
        
        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.Context.AgeChanged.Should().BeFalse();
        result.Context.NewAge.Should().Be(originalAge);
        result.Context.OldAge.Should().Be(originalAge);
    }
}