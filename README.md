# FluentPatcher

<p align="center">
	<img src=".github/images/logo.png" alt="Logo" style="width: 10%">
</p>

FluentPatcher is small .NET library for applying partial updates (patches) to your entities and tracking the changes made during the patching process.

## Installation
Add FluentPatcher to your project via NuGet or the .NET CLI:

```bash
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

[PatchFor(typeof(User))]
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
};

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

## Usage with ASP.NET

In order to use `FluentPatcher` with ASP.NET, you should register a `PatchableJsonConverterFactory`.

This will allow you to deserialize `Patchable<T>` properties from JSON in HTTP request bodies.

```cs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new PatchableJsonConverterFactory());
});
```

This repo includes an example ASP.NET project: `FluentPatcher.Sample`.

## How to get patch summary

After applying a patch you can:

- Access changed values as a dictionary with `GetChangedValues()`
- Get a human-readable string with `GetChangesSummary()`

Example (HTTP PATCH request):

```http
### Partially update user — set Email to null and update Age
PATCH {{host}}/user
Content-Type: application/json

{
  "email": null,
  "age": 35
}
```

Expected output:

```json
{
  "changes": {
    "Email": null,
    "Age": 35
  },
  "summary": "Email: 'john.doe@example.com' -> null\nAge: '30' -> '35'"
}
```

## License

FluentPatcher is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.