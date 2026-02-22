namespace FluentPatcher.Tests.Models;

/// <summary>
/// Represents an address model used in testing patching of complex types and collections.
/// </summary>
public sealed class Address
{
    /// <summary>
    /// Street address.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// City name.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Country name.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Postal code.
    /// </summary>
    public string? PostalCode { get; set; }
}