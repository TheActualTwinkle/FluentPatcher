namespace FluentPatcher.Tests.Models;

/// <summary>
/// Entity representing a user with various properties to test patching behavior.
/// </summary>
public class UserEntity
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string? Email { get; set; } = string.Empty;

    /// <summary>
    /// User's age.
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// User's home address.
    /// </summary>
    public Address? HomeAddress { get; set; }

    /// <summary>
    /// List of user's work addresses.
    /// </summary>
    public List<Address> WorkAddresses { get; set; } = [];

    /// <summary>
    /// Timestamp of when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}