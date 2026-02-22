namespace FluentPatcher.Generator.Models;

/// <summary>
/// Model for collection patching strategy.
/// </summary>
internal sealed class CollectionStrategyModel
{
    public string Strategy { get; set; } = "Replace";

    public string? KeyProperty { get; set; }

    public string? ItemTypeName { get; set; }
}