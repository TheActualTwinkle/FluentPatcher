using FluentAssertions;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for applying patches to entities that contain nested complex models,
/// and verifying how nested objects are created, updated, or left unchanged depending on the patch data.
/// </summary>
public sealed class NestedModelsPatchTests
{
    /// <summary>
    /// Tests that when a patch provides values for a nested model property a new nested instance is created (if needed)
    /// and its properties are updated, while the original entity instance remains unchanged.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenNestedModelUpdate_ShouldCreateAndUpdateNestedModel()
    {
        // Arrange.
        var originalAddress = new Address
        {
            Street = "Old street",
            City = "Old city",
            Country = "Old country",
            PostalCode = "00000"
        };

        var user = new UserEntity
        {
            Name = "Original",
            HomeAddress = originalAddress
        };

        var newAddress = new Address
        {
            Street = "New street",
            City = "New city",
            Country = "New country",
            PostalCode = "11111"
        };

        var patch = new UserUpdateDto
        {
            WorkAddresses = new List<Address> { newAddress }
        };

        // Act.
        var result = patch.ApplyTo(user);

        // Assert.
        result.HasChanges.Should().BeTrue();

        result.Entity.Should().NotBeSameAs(user);

        result.Entity.WorkAddresses.Should().ContainSingle();
        result.Entity.WorkAddresses.First().Should().Be(newAddress);
    }

    /// <summary>
    /// Tests that when updating a nested model, unrelated properties on the root entity and on other nested models remain unchanged.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenUpdatingNestedModel_ShouldNotChangeOtherProperties()
    {
        // Arrange.
        var originalHomeAddress = new Address
        {
            Street = "Home street",
            City = "Home city",
            Country = "Home country",
            PostalCode = "00000"
        };

        var originalWorkAddress = new Address
        {
            Street = "Work street",
            City = "Work city",
            Country = "Work country",
            PostalCode = "11111"
        };

        var originalUser = new UserEntity
        {
            Name = "Original name",
            Age = 42,
            HomeAddress = originalHomeAddress,
            WorkAddresses = [originalWorkAddress]
        };

        var patch = new UserUpdateDto
        {
            HomeAddress = new Address
            {
                Street = "New home street",
                City = "New home city",
                Country = "New home country",
                PostalCode = "99999"
            }
        };

        // Act.
        var result = patch.ApplyTo(originalUser);
        
        // Assert.
        result.HasChanges.Should().BeTrue();
        
        result.Entity.Should().BeEquivalentTo(originalUser, options => options.Excluding(u => u.HomeAddress));
    }
}
