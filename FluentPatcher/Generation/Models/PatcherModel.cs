namespace FluentPatcher.Generator.Models;

/// <summary>
/// Model representing a DTO class to generate patcher for.
/// </summary>
internal sealed class PatcherModel
{
    public string Namespace { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string? TargetEntityTypeName { get; set; }

    public string? CustomPatcherName { get; set; }

    public List<PropertyModel> Properties { get; set; } = [];

    public string PatcherClassName =>
        CustomPatcherName ?? $"{ClassName}Patcher";

    public string ContextClassName =>
        $"{ClassName}PatchContext";

    public string ExtensionClassName =>
        $"{ClassName}Extensions";
}