using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentPatcher.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FluentPatcherAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor NonPatchablePropertyError = new(
        "FP0001",
        "Property must be Patchable<T>",
        "Property '{0}' in patch class '{1}' must be of type Patchable<T>. Use Patchable<{2}> instead of {2}.",
        "FluentPatcher",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor UnmatchedPropertyWarning = new(
        "FP0002",
        "Patch property does not match any target entity property",
        "Property '{0}' in patch class '{1}' does not match any property on target entity '{2}'. Either rename it to match or specify PatchProperty(TargetPropertyName = nameof({2}.<Property>)).",
        "FluentPatcher",
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor PatchableInnerTypeNullabilityMismatchError = new(
        "FP0003",
        "Patchable inner type nullability must match target property",
        "Property '{0}' in patch class '{1}' uses Patchable<{2}>, but target property '{3}' on '{4}' has type '{5}'. Use Patchable<{5}>.",
        "FluentPatcher",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [NonPatchablePropertyError, UnmatchedPropertyWarning, PatchableInnerTypeNullabilityMismatchError];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
    }

    private void AnalyzeClass(SymbolAnalysisContext context)
    {
        var classSymbol = (INamedTypeSymbol)context.Symbol;

        if (!classSymbol.GetAttributes().Any(a => a.AttributeClass?.Name is "PatchForAttribute" or "PatchFor"))
            return;

        var targetEntitySymbol = GetTargetEntitySymbol(classSymbol);
        var targetEntityTypeName = targetEntitySymbol?.ToDisplayString();

        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
                continue;

            if (propertySymbol.DeclaredAccessibility != Accessibility.Public ||
                propertySymbol.IsStatic ||
                propertySymbol.IsIndexer)
                continue;

            var isPatchable = IsPatchable(propertySymbol.Type);

            if (!isPatchable)
            {
                var typeName = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var diag = Diagnostic.Create(
                    NonPatchablePropertyError,
                    propertySymbol.Locations.FirstOrDefault(),
                    propertySymbol.Name,
                    classSymbol.Name,
                    typeName);

                context.ReportDiagnostic(diag);
                continue;
            }

            if (targetEntitySymbol is null)
                continue;

            var targetProperty = GetTargetPropertySymbol(propertySymbol, targetEntitySymbol);
            if (targetProperty is null)
            {
                var diag = Diagnostic.Create(
                    UnmatchedPropertyWarning,
                    propertySymbol.Locations.FirstOrDefault(),
                    propertySymbol.Name,
                    classSymbol.Name,
                    targetEntityTypeName ?? targetEntitySymbol.ToDisplayString(),
                    GetTargetPropertyName(propertySymbol));

                context.ReportDiagnostic(diag);
                continue;
            }

            if (!HasMatchingNullability(propertySymbol.Type, targetProperty.Type))
            {
                var actualInnerTypeName = GetPatchableInnerTypeDisplayName(propertySymbol.Type);
                var targetTypeName = GetTypeDisplayName(targetProperty.Type);
                var diag = Diagnostic.Create(
                    PatchableInnerTypeNullabilityMismatchError,
                    propertySymbol.Locations.FirstOrDefault(),
                    propertySymbol.Name,
                    classSymbol.Name,
                    actualInnerTypeName,
                    targetProperty.Name,
                    targetEntityTypeName ?? targetEntitySymbol.ToDisplayString(),
                    targetTypeName);

                context.ReportDiagnostic(diag);
            }
        }
    }

    private static INamedTypeSymbol? GetTargetEntitySymbol(INamedTypeSymbol classSymbol)
    {
        var patchAttr = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "PatchForAttribute" or "PatchFor");

        if (patchAttr is null)
            return null;

        if (patchAttr.ConstructorArguments.Length > 0 &&
            patchAttr.ConstructorArguments[0].Value is INamedTypeSymbol ctorTypeSymbol)
            return ctorTypeSymbol;

        foreach (var namedArg in patchAttr.NamedArguments)
            if (namedArg.Key == "TargetEntityType" &&
                namedArg.Value.Value is INamedTypeSymbol typeSymbol)
                return typeSymbol;

        return null;
    }

    private static IPropertySymbol? GetTargetPropertySymbol(IPropertySymbol patchProperty, INamedTypeSymbol targetEntitySymbol)
    {
        var targetName = GetTargetPropertyName(patchProperty);

        return targetEntitySymbol.GetMembers().OfType<IPropertySymbol>()
            .FirstOrDefault(p =>
                p.Name == targetName &&
                p.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal &&
                !p.IsStatic);
    }

    private static string GetTargetPropertyName(IPropertySymbol patchProperty)
    {
        var patchPropertyAttr = patchProperty.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "PatchPropertyAttribute" or "PatchProperty");

        if (patchPropertyAttr != null)
            foreach (var namedArg in patchPropertyAttr.NamedArguments)
                if (namedArg.Key == "TargetPropertyName" &&
                    namedArg.Value.Value is string s &&
                    !string.IsNullOrWhiteSpace(s))
                    return s;

        return patchProperty.Name;
    }

    private static bool IsPatchable(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var typeName = namedType.OriginalDefinition.ToDisplayString();

        return typeName == "FluentPatcher.Patchable<T>" ||
               (namedType.Name == "Patchable" && namedType.TypeArguments.Length == 1);
    }

    private static bool HasMatchingNullability(ITypeSymbol patchableType, ITypeSymbol targetType)
    {
        if (patchableType is not INamedTypeSymbol namedPatchable || namedPatchable.TypeArguments.Length != 1)
            return true;

        var patchInnerType = namedPatchable.TypeArguments[0];
        var patchNullability = GetNullabilityKind(patchInnerType);
        var targetNullability = GetNullabilityKind(targetType);

        return patchNullability == NullabilityKind.Unknown ||
               targetNullability == NullabilityKind.Unknown ||
               patchNullability == targetNullability;
    }

    private static string GetPatchableInnerTypeDisplayName(ITypeSymbol patchableType)
    {
        if (patchableType is INamedTypeSymbol namedPatchable && namedPatchable.TypeArguments.Length == 1)
            return GetTypeDisplayName(namedPatchable.TypeArguments[0]);

        return patchableType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    private static string GetTypeDisplayName(ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));

    private static NullabilityKind GetNullabilityKind(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            return NullabilityKind.Nullable;

        if (type.IsReferenceType)
            return type.NullableAnnotation switch
            {
                NullableAnnotation.Annotated => NullabilityKind.Nullable,
                NullableAnnotation.NotAnnotated => NullabilityKind.NonNullable,
                _ => NullabilityKind.Unknown
            };

        if (type.IsValueType)
            return NullabilityKind.NonNullable;

        return NullabilityKind.Unknown;
    }

    private enum NullabilityKind
    {
        Unknown,
        NonNullable,
        Nullable
    }
}

