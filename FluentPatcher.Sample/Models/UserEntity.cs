namespace FluentPatcher.Sample.Models;

public class UserEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public int? Age { get; set; }

    public Address? HomeAddress { get; set; }

    public List<Address> WorkAddresses { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}

public sealed class Address
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? PostalCode { get; set; }
}