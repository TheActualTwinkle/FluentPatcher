using FluentPatcher.Attributes;

namespace FluentPatcher.Tests.Models;

/// <summary>
/// Patch request for <see cref="UserEntity"/>, used to test patching behavior.
/// </summary>
[PatchFor(typeof(UserEntity))]
public class UserUpdateDto
{
    /// <summary>
    /// New user name.
    /// </summary>
    public Patchable<string?> Name { get; init; }

    /// <summary>
    /// New user email.
    /// </summary>
    public Patchable<string?> Email { get; init; }

    /// <summary>
    /// New user age.
    /// </summary>
    public Patchable<int?> Age { get; init; }

    /// <summary>
    /// New work addresses for the user.
    /// </summary>
    public Patchable<List<Address>?> WorkAddresses { get; init; }

    /// <summary>
    /// New home address for the user.
    /// </summary>
    public Patchable<Address?> HomeAddress { get; init; }
}