using FluentAssertions;
using FluentPatcher.Tests.Models;
using Xunit;

namespace FluentPatcher.Tests;

/// <summary>
/// Tests for applying patches to collection properties using supported <see cref="FluentPatcher.Attributes.CollectionPatchStrategy"/>
/// via dedicated patch models with explicit collection strategy configuration.
/// </summary>
public sealed class CollectionPatchStrategyTests
{
    /// <summary>
    /// Tests that when using the default <see cref="FluentPatcher.Attributes.CollectionPatchStrategy.Replace"/> strategy
    /// on a collection, the existing collection on the entity is completely replaced with the new collection from the patch.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenUsingReplaceStrategy_ShouldReplaceEntireCollection()
    {
        // Arrange.
        var originalItem = new Address
        {
            Street = "Old street",
            City = "Old city",
            Country = "Old country",
            PostalCode = "00000"
        };

        var entity = new CollectionStrategyEntity
        {
            ReplaceItems = [originalItem]
        };

        var newItem1 = new Address
        {
            Street = "New street 1",
            City = "New city 1",
            Country = "New country 1",
            PostalCode = "11111"
        };

        var newItem2 = new Address
        {
            Street = "New street 2",
            City = "New city 2",
            Country = "New country 2",
            PostalCode = "22222"
        };

        var patch = new CollectionStrategyReplaceUpdateDto
        {
            ReplaceItems = new List<Address> { newItem1, newItem2 }
        };

        // Act.
        var result = patch.ApplyTo(entity);

        // Assert.
        result.HasChanges.Should().BeTrue();
        
        result.Entity.ReplaceItems.Should().HaveCount(2);
        result.Entity.ReplaceItems.Should().ContainInOrder(newItem1, newItem2);
    }

    /// <summary>
    /// Tests that collection change detection compares items by default instead of collection references.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenCollectionItemsAreEqual_ShouldNotMarkCollectionChanged()
    {
        // Arrange.
        var tags = new List<string> { "stable", "ordered" };
        var entity = new CollectionComparisonEntity
        {
            Tags = tags
        };

        var patch = new CollectionComparisonUpdateDto
        {
            Tags = new List<string> { "stable", "ordered" }
        };

        // Act.
        var result = patch.ApplyTo(entity);

        // Assert.
        result.HasChanges.Should().BeFalse();
        result.Context.TagsChanged.Should().BeFalse();
        result.Entity.Tags.Should().BeSameAs(tags);
    }

    /// <summary>
    /// Tests that ordered item differences are detected by default collection comparison.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenCollectionItemsHaveDifferentOrder_ShouldMarkCollectionChanged()
    {
        // Arrange.
        var entity = new CollectionComparisonEntity
        {
            Tags = new List<string> { "stable", "ordered" }
        };

        var patch = new CollectionComparisonUpdateDto
        {
            Tags = new List<string> { "ordered", "stable" }
        };

        // Act.
        var result = patch.ApplyTo(entity);

        // Assert.
        result.HasChanges.Should().BeTrue();
        result.Context.TagsChanged.Should().BeTrue();
        result.Entity.Tags.Should().Equal("ordered", "stable");
    }

    /// <summary>
    /// Tests that the global collection comparison mode can restore reference-based change detection.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenGlobalCollectionComparisonIsReference_ShouldMarkDifferentCollectionInstanceChanged()
    {
        // Arrange.
        var previousComparison = PatchOptions.DefaultCollectionComparison;
        PatchOptions.DefaultCollectionComparison = CollectionChangeComparison.Reference;

        try
        {
            var entity = new CollectionComparisonEntity
            {
                Tags = new List<string> { "stable", "ordered" }
            };

            var patch = new CollectionComparisonUpdateDto
            {
                Tags = new List<string> { "stable", "ordered" }
            };

            // Act.
            var result = patch.ApplyTo(entity);

            // Assert.
            result.HasChanges.Should().BeTrue();
            result.Context.TagsChanged.Should().BeTrue();
        }
        finally
        {
            PatchOptions.DefaultCollectionComparison = previousComparison;
        }
    }

    /// <summary>
    /// Tests that per-call options override the global collection comparison mode.
    /// </summary>
    [Fact]
    public void ApplyPatch_WhenLocalCollectionComparisonOverridesGlobalReference_ShouldUseLocalComparison()
    {
        // Arrange.
        var previousComparison = PatchOptions.DefaultCollectionComparison;
        PatchOptions.DefaultCollectionComparison = CollectionChangeComparison.Reference;

        try
        {
            var entity = new CollectionComparisonEntity
            {
                Tags = new List<string> { "stable", "ordered" }
            };

            var patch = new CollectionComparisonUpdateDto
            {
                Tags = new List<string> { "stable", "ordered" }
            };

            var options = new PatchOptions
            {
                CollectionComparison = CollectionChangeComparison.Sequence
            };

            // Act.
            var result = patch.ApplyTo(entity, options: options);

            // Assert.
            result.HasChanges.Should().BeFalse();
            result.Context.TagsChanged.Should().BeFalse();
        }
        finally
        {
            PatchOptions.DefaultCollectionComparison = previousComparison;
        }
    }

    // /// <summary>
    // /// Tests collection patching using <see cref="CollectionPatchStrategy.Append"/> strategy.
    // /// The test verifies that when applying a patch with Append strategy,
    // /// the new items from the patch are added to the existing collection on the entity without removing any existing items.
    // /// </summary>
    // [Fact]
    // public void ApplyPatch_WhenUsingAppendStrategy_CurrentImplementation_ShouldAppendCollection()
    // {
    //     // Arrange.
    //     var existing = new Address
    //     {
    //         Street = "Existing",
    //         City = "City",
    //         Country = "Country",
    //         PostalCode = "0000"
    //     };
    //
    //     var entity = new CollectionStrategyEntity
    //     {
    //         AppendItems = [existing]
    //     };
    //
    //     var newItem1 = new Address
    //     {
    //         Street = "New 1",
    //         City = "City 1",
    //         Country = "Country",
    //         PostalCode = "1111"
    //     };
    //
    //     var newItem2 = new Address
    //     {
    //         Street = "New 2",
    //         City = "City 2",
    //         Country = "Country",
    //         PostalCode = "2222"
    //     };
    //
    //     var patch = new CollectionStrategyAppendPatch
    //     {
    //         AppendItems = new List<Address> { newItem1, newItem2 }
    //     };
    //     
    //     // Act.
    //     var result = patch.ApplyTo(entity);
    //
    //     // Assert.
    //     result.HasChanges.Should().BeTrue();
    //
    //     result.Entity.AppendItems.Should().HaveCount(3);
    //     result.Entity.AppendItems.Should().Contain(newItem1);
    //     result.Entity.AppendItems.Should().Contain(newItem2);
    //     result.Entity.AppendItems.Should().Contain(existing);
    // }
}
