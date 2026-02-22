namespace FluentPatcher.Generator.Models;

/// <summary>
/// Model representing a property to patch.
/// </summary>
internal sealed class PropertyModel
{
    public string Name { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public string FullTypeName { get; set; } = string.Empty;

    public bool IsCollection { get; set; }
    
    public bool IsPatchable { get; set; }

    public string? PatchableInnerType { get; set; }

    public string? TargetPropertyName { get; set; }

    public string? ComparerTypeName { get; set; }

    public CollectionStrategyModel? CollectionStrategy { get; set; }

    public string EffectiveTargetName =>
        TargetPropertyName ?? Name;
}