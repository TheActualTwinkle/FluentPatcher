using FluentPatcher.Attributes;

namespace FluentPatcher.Sample.Models;

[PatchFor(typeof(UserEntity))]
public class UserUpdateDto
{
    public Patchable<string?> Name { get; init; }

    public Patchable<string?> Email { get; init; }

    public Patchable<int?> Age { get; init; }

    public Patchable<List<Address>?> WorkAddresses { get; init; }

    public Patchable<Address?> HomeAddress { get; init; }
}