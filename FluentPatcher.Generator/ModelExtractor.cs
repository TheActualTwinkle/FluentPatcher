using System.Linq;
using FluentPatcher.Generator.Models;
using Microsoft.CodeAnalysis;

namespace FluentPatcher.Generator;

internal static class ModelExtractor
{
    private const string PatchPropertyAttributeName = "PatchPropertyAttribute";
    private const string CollectionPatchStrategyAttributeName = "CollectionPatchStrategyAttribute";

    public static PatcherModel ExtractModel(INamedTypeSymbol classSymbol)
    {
        var model = new PatcherModel
        {
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName = classSymbol.Name
        };

        // Extract attribute data
        var patchAttr = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "PatchForAttribute" or "PatchFor");

        if (patchAttr is not null)
        {
            // Positional constructor argument: target entity type
            if (patchAttr.ConstructorArguments.Length > 0 &&
                patchAttr.ConstructorArguments[0].Value is INamedTypeSymbol ctorTypeSymbol)
            {
                model.TargetEntityTypeName = ctorTypeSymbol.ToDisplayString();
            }

            foreach (var namedArg in patchAttr.NamedArguments)
                switch (namedArg.Key)
                {
                    case "TargetEntityType":
                        if (namedArg.Value.Value is INamedTypeSymbol typeSymbol)
                            model.TargetEntityTypeName = typeSymbol.ToDisplayString();

                        break;
                    case "PatcherName":
                        model.CustomPatcherName = namedArg.Value.Value?.ToString();
                        break;
                }
        }

        // Extract properties
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IPropertySymbol propertySymbol)
                continue;

            // Skip non-public or static
            if (propertySymbol.DeclaredAccessibility != Accessibility.Public ||
                propertySymbol.IsStatic)
                continue;

            // Skip indexers
            if (propertySymbol.IsIndexer)
                continue;

            var propertyModel = ExtractPropertyModel(propertySymbol);
            
            if (propertyModel != null)
                model.Properties.Add(propertyModel);
        }

        return model;
    }

    private static PropertyModel? ExtractPropertyModel(IPropertySymbol propertySymbol)
    {
        var (isPatchable, patchableInnerType) = CheckPatchableType(propertySymbol.Type);

        // All properties must be Patchable<T>
        if (!isPatchable)
            return null;

        var model = new PropertyModel
        {
            Name = propertySymbol.Name,
            TypeName = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            FullTypeName = propertySymbol.Type.ToDisplayString(),
            IsCollection = false,
            IsPatchable = true,
            PatchableInnerType = patchableInnerType
        };

        // Determine if the patchable inner type is a collection (e.g., Patchable<List<T>>)
        if (propertySymbol.Type is INamedTypeSymbol namedType &&
            namedType.TypeArguments.Length == 1)
        {
            var innerTypeSymbol = namedType.TypeArguments[0];
            if (IsCollectionType(innerTypeSymbol))
                model.IsCollection = true;
        }

        // Process attributes
        foreach (var attr in propertySymbol.GetAttributes())
        {
            var attrName = attr.AttributeClass?.Name;
            switch (attrName)
            {
                case PatchPropertyAttributeName:
                case "PatchProperty":
                    ProcessPatchPropertyAttribute(attr, model);
                    break;
                case CollectionPatchStrategyAttributeName:
                case "CollectionPatchStrategy":
                    if (model.IsCollection)
                        model.CollectionStrategy = ProcessCollectionStrategyAttribute(attr, propertySymbol.Type);
                    break;
            }
        }

        return model;
    }

    private static void ProcessPatchPropertyAttribute(AttributeData attr, PropertyModel model)
    {
        foreach (var namedArg in attr.NamedArguments)
            switch (namedArg.Key)
            {
                case "TargetPropertyName":
                    model.TargetPropertyName = namedArg.Value.Value?.ToString();
                    break;
                
                case "ComparerType":
                    if (namedArg.Value.Value is INamedTypeSymbol comparerType)
                        model.ComparerTypeName = comparerType.ToDisplayString();

                    break;
            }
    }

    private static CollectionStrategyModel ProcessCollectionStrategyAttribute(AttributeData attr, ITypeSymbol propertyType)
    {
        var strategy = new CollectionStrategyModel
        {
            ItemTypeName = GetCollectionItemType(propertyType)
        };

        foreach (var namedArg in attr.NamedArguments)
            switch (namedArg.Key)
            {
                case "Strategy":
                    // Enum value - get the field name
                    if (namedArg.Value.Value is int intValue)
                        strategy.Strategy = intValue switch
                        {
                            _ => "Replace"
                        };

                    break;
                case "KeyProperty":
                    strategy.KeyProperty = namedArg.Value.Value?.ToString();

                    break;
            }

        return strategy;
    }

    private static bool IsCollectionType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return false;

        // Unwrap nullable
        if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            namedType = (INamedTypeSymbol)namedType.TypeArguments[0];

        return namedType.AllInterfaces.Any(i =>
            i.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>");
    }

    private static string? GetCollectionItemType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return null;

        // If this is Patchable<T>, unwrap one level to get T
        // Common case: property type is Patchable<List<T>>
        if (namedType.OriginalDefinition.ToDisplayString() == "FluentPatcher.Patchable<T>" ||
            (namedType.Name == "Patchable" && namedType.TypeArguments.Length == 1))
        {
            var inner = namedType.TypeArguments[0];

            if (inner is INamedTypeSymbol innerNamed)
                namedType = innerNamed;
            else
                return null;
        }

        // Unwrap nullable (T?)
        if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            namedType.TypeArguments.Length > 0 &&
            namedType.TypeArguments[0] is INamedTypeSymbol nullableInner)
            namedType = nullableInner;

        // Now if this is a generic collection (e.g., List<T>, IEnumerable<T>), return the element type
        if (namedType.TypeArguments.Length > 0)
            return namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        return null;
    }

    private static (bool IsPatchable, string? InnerType) CheckPatchableType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return (false, null);

        var typeName = namedType.OriginalDefinition.ToDisplayString();

        // Check for Patchable<T>
        if (typeName == "FluentPatcher.Patchable<T>" ||
            namedType is { Name: "Patchable", TypeArguments.Length: 1 })
        {
            var innerType = namedType.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            return (true, innerType);
        }

        return (false, null);
    }
}