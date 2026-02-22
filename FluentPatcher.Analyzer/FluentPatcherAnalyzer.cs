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

    private static readonly DiagnosticDescriptor PatchableInnerTypeMustBeNullableError = new(
        "FP0003",
        "Patchable inner type must be nullable",
        "Property '{0}' in patch class '{1}' uses Patchable<{2}>, but '{2}' must be nullable",
        "FluentPatcher",
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [NonPatchablePropertyError, UnmatchedPropertyWarning, PatchableInnerTypeMustBeNullableError];

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

        var patchAttr = classSymbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name is "PatchForAttribute" or "PatchFor");
        INamedTypeSymbol? targetEntitySymbol = null;
        string? targetEntityTypeName = null;

        if (patchAttr != null)
        {
            if (patchAttr.ConstructorArguments.Length > 0 &&
                patchAttr.ConstructorArguments[0].Value is INamedTypeSymbol ctorTypeSymbol)
            {
                targetEntitySymbol = ctorTypeSymbol;
                targetEntityTypeName = ctorTypeSymbol.ToDisplayString();
            }

            foreach (var namedArg in patchAttr.NamedArguments)
                if (namedArg.Key == "TargetEntityType" &&
                    namedArg.Value.Value is INamedTypeSymbol typeSymbol)
                {
                    targetEntitySymbol = typeSymbol;
                    targetEntityTypeName = typeSymbol.ToDisplayString();
                }
        }

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
                    NonPatchablePropertyError, propertySymbol.Locations.FirstOrDefault(), propertySymbol.Name, classSymbol.Name, typeName);

                context.ReportDiagnostic(diag);

                continue;
            }

            if (propertySymbol.Type is INamedTypeSymbol patchableType &&
                patchableType.TypeArguments.Length == 1)
            {
                var innerType = patchableType.TypeArguments[0];

                if (!IsNullableType(innerType))
                {
                    var innerTypeName = innerType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                    var diag = Diagnostic.Create(
                        PatchableInnerTypeMustBeNullableError, propertySymbol.Locations.FirstOrDefault(), propertySymbol.Name, classSymbol.Name,
                        innerTypeName);

                    context.ReportDiagnostic(diag);
                }
            }

            if (targetEntitySymbol is not null)
            {
                var targetName = propertySymbol.Name;

                var patchPropertyAttr = propertySymbol.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.Name is "PatchPropertyAttribute" or "PatchProperty");

                if (patchPropertyAttr != null)
                    foreach (var namedArg in patchPropertyAttr.NamedArguments)
                        if (namedArg.Key == "TargetPropertyName" &&
                            namedArg.Value.Value is string s &&
                            !string.IsNullOrWhiteSpace(s))
                            targetName = s;

                var hasMatch = targetEntitySymbol.GetMembers().OfType<IPropertySymbol>()
                    .Any(p => p.Name == targetName && p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

                if (!hasMatch)
                {
                    var diag = Diagnostic.Create(
                        UnmatchedPropertyWarning, propertySymbol.Locations.FirstOrDefault(), propertySymbol.Name, classSymbol.Name,
                        targetEntityTypeName ?? targetEntitySymbol.ToDisplayString(), targetName);

                    context.ReportDiagnostic(diag);
                }
            }
        }
    }

    private static bool IsPatchable(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        var typeName = namedType.OriginalDefinition.ToDisplayString();

        if (typeName == "FluentPatcher.Patchable<T>" ||
            (namedType.Name == "Patchable" && namedType.TypeArguments.Length == 1))
            return true;

        return false;
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return true;

        if (type is INamedTypeSymbol namedType &&
            namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            return true;

        return false;
    }
}