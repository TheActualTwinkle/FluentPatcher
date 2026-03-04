// using FluentPatcher;
// using FluentPatcher.Sample.Models;
//
// var builder = WebApplication.CreateBuilder(args);
//
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.Converters.Add(new PatchableJsonConverterFactory());
// });
//
// var app = builder.Build();
//
// var user = new UserEntity
// {
//     Id = Guid.NewGuid(),
//     Name = "John Doe",
//     Email = "john.doe@example.com",
//     Age = 30,
//     HomeAddress = new Address
//     {
//         Street = "Main st. 1",
//         City = "New York",
//         Country = "USA",
//         PostalCode = "12345"
//     },
//     WorkAddresses =
//     [
//         new Address
//         {
//             Street = "Work st. 10",
//             City = "New York",
//             Country = "USA",
//             PostalCode = "54321"
//         }
//     ],
//     CreatedAt = DateTime.UtcNow
// };
//
// app.MapPost(
//     "/user", () => Results.Created($"/user/{user.Id}", user));
//
// app.MapPatch(
//     "/user", (UserUpdateDto patch) =>
//     {
//         var result = patch.ApplyTo(user);
//
//         var changedValues = result.Context.GetChangedValues();
//         var summary = result.Context.GetChangesSummary();
//
//         Console.WriteLine("PATCH /user changes summary:");
//         Console.WriteLine(summary);
//
//         return Results.Ok(
//             new
//             {
//                 result.HasChanges,
//                 NewEntity = result.Entity,
//                 Changes = changedValues,
//                 Summary = summary
//             });
//     });
//
// app.Run();