# FluentPatcher

<p align="center">
	<img src=".github/images/logo.png" alt="Logo" style="width: 10%">
</p>

FluentPatcher is small .NET library for applying partial updates (patches) to your entities and tracking the changes made during the patching process.

## Installation
Add FluentPatcher to your project via NuGet or the .NET CLI:

```cs
dotnet add package FluentPatcher
```

## Quick Example

Lest's say you have some `User` entity and a corresponding `UserUpdateDto` for updates:

```cs
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[PatchFor(typeof(UserEntity))]
public class UserUpdateDto
{
    public Patchable<string> Name { get; set; }
}
```

Then you can do this:

```cs
var originalUser = new User
{
    Id = 1,
    Name = "Bob"
}

var patch = new UserUpdateDto
{
    Name = "Alice"
};

var result = patch.ApplyTo(originalUser);

Console.WriteLine(result.HasChanges); // Output: true
Console.WriteLine(result.Entity.Name); // Output: "Alice"

Console.WriteLine(result.Context.NameChanged); // Output: true
Console.WriteLine(result.Context.OldName); // Output: "Bob"
Console.WriteLine(result.Context.NewName); // Output: "Alice"
```

## License

FluentPatcher is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.