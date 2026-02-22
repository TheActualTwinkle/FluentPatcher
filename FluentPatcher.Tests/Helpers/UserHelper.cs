using FluentPatcher.Tests.Models;

namespace FluentPatcher.Tests.Helpers;

/// <summary>
/// Helper class for managing user entities in tests.
/// </summary>
public static class UserHelper
{
    /// <summary>
    /// Creates a new instance of <see cref="UserEntity"/> with default values for testing purposes.
    /// </summary>
    /// <returns>A new <see cref="UserEntity"/> instance.</returns>
    public static UserEntity CreateUser() =>
        new()
        {
            Id = Guid.CreateVersion7(),
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };
}