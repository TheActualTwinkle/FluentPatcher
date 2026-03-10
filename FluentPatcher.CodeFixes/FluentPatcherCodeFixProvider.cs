using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentPatcher.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FluentPatcherCodeFixProvider)), Shared]
public sealed class FluentPatcherCodeFixProvider : CodeFixProvider
{
    private const string Fp0001 = "FP0001";
    private const string Fp0002 = "FP0002";
    private const string Fp0003 = "FP0003";

    private static readonly SymbolDisplayFormat MinimalTypeDisplayFormat = SymbolDisplayFormat.MinimallyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                                  SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
                                  SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public override ImmutableArray<string> FixableDiagnosticIds =>
        [Fp0001, Fp0002, Fp0003];

    public override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic == null)
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
            return;

        var node = root.FindNode(diagnostic.Location.SourceSpan);

        switch (diagnostic.Id)
        {
            case Fp0001:
                if (node is PropertyDeclarationSyntax property)
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Wrap in Patchable<T>",
                            _ => Task.FromResult(WrapPropertyTypeInPatchable(context.Document, root, property)),
                            nameof(FluentPatcherCodeFixProvider) + "_FP0001"),
                        diagnostic);

                break;

            case Fp0002:
                var propForAttribute = node.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                if (propForAttribute != null)
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Add [PatchProperty(TargetPropertyName = ...)]",
                            _ => Task.FromResult(AddPatchPropertyAttribute(context.Document, root, propForAttribute)),
                            nameof(FluentPatcherCodeFixProvider) + "_FP0002"),
                        diagnostic);

                break;

            case Fp0003:
                var propForNullability = node.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                if (propForNullability != null)
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Match Patchable<T> nullability to target property",
                            _ => MatchPatchableInnerTypeToTargetPropertyAsync(context.Document, root, propForNullability),
                            nameof(FluentPatcherCodeFixProvider) + "_FP0003"),
                        diagnostic);

                break;
        }
    }

    private static Document WrapPropertyTypeInPatchable(
        Document document,
        SyntaxNode root,
        PropertyDeclarationSyntax property)
    {
        var originalType = property.Type;

        var patchableType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("Patchable"))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(originalType.WithoutTrivia())));

        var newProperty = property.WithType(patchableType.WithTriviaFrom(originalType));
        var newRoot = root.ReplaceNode(property, newProperty);

        return document.WithSyntaxRoot(newRoot);
    }

    private static Document AddPatchPropertyAttribute(
        Document document,
        SyntaxNode root,
        PropertyDeclarationSyntax property)
    {
        if (property.Parent is not ClassDeclarationSyntax classDecl)
            return document;

        var patchForAttr = classDecl.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains("PatchFor"));

        var entityTypeName = "Entity";

        if (patchForAttr?.ArgumentList?.Arguments.FirstOrDefault() is { Expression: TypeOfExpressionSyntax typeOfExpr })
        {
            if (typeOfExpr.Type is IdentifierNameSyntax id)
                entityTypeName = id.Identifier.Text;
            else if (typeOfExpr.Type is QualifiedNameSyntax qn)
                entityTypeName = qn.Right.Identifier.Text;
            else if (typeOfExpr.Type is GenericNameSyntax gn)
                entityTypeName = gn.Identifier.Text;
        }

        const string targetPropertyName = "TARGET_PROPERTY";
        var targetNameof = SyntaxFactory.ParseExpression($"nameof({entityTypeName}.{targetPropertyName})");

        var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName("PatchProperty"))
            .WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.NameEquals("TargetPropertyName"),
                            null,
                            targetNameof))));

        var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute))
            .WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        var newAttrLists = property.AttributeLists.Add(attributeList);

        var newProperty = property.WithAttributeLists(newAttrLists);
        var newRoot = root.ReplaceNode(property, newProperty);

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> MatchPatchableInnerTypeToTargetPropertyAsync(
        Document document,
        SyntaxNode root,
        PropertyDeclarationSyntax property)
    {
        if (property.Type is not GenericNameSyntax genericName ||
            genericName.Identifier.Text != "Patchable" ||
            genericName.TypeArgumentList.Arguments.Count != 1)
            return document;

        var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var propertySymbol = semanticModel.GetDeclaredSymbol(property) as IPropertySymbol;
        if (propertySymbol is null)
            return document;

        var targetProperty = GetTargetPropertySymbol(propertySymbol);
        if (targetProperty is null)
            return document;

        var replacementTypeName = targetProperty.Type.ToMinimalDisplayString(semanticModel, property.SpanStart, MinimalTypeDisplayFormat);
        var replacementType = SyntaxFactory.ParseTypeName(replacementTypeName)
            .WithTriviaFrom(genericName.TypeArgumentList.Arguments[0]);

        var newGeneric = genericName.WithTypeArgumentList(
            SyntaxFactory.TypeArgumentList(
                SyntaxFactory.SingletonSeparatedList(replacementType)));

        var newProperty = property.WithType(newGeneric.WithTriviaFrom(property.Type));
        var newRoot = root.ReplaceNode(property, newProperty);

        return document.WithSyntaxRoot(newRoot);
    }

    private static IPropertySymbol? GetTargetPropertySymbol(IPropertySymbol patchProperty)
    {
        var patchClass = patchProperty.ContainingType;
        var targetEntity = GetTargetEntitySymbol(patchClass);
        if (targetEntity is null)
            return null;

        var targetPropertyName = GetTargetPropertyName(patchProperty);

        return targetEntity.GetMembers().OfType<IPropertySymbol>()
            .FirstOrDefault(p =>
                p.Name == targetPropertyName &&
                p.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal &&
                !p.IsStatic);
    }

    private static INamedTypeSymbol? GetTargetEntitySymbol(INamedTypeSymbol patchClass)
    {
        var patchAttr = patchClass.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name is "PatchForAttribute" or "PatchFor");

        if (patchAttr is null)
            return null;

        if (patchAttr.ConstructorArguments.Length > 0 &&
            patchAttr.ConstructorArguments[0].Value is INamedTypeSymbol ctorTypeSymbol)
            return ctorTypeSymbol;

        foreach (var namedArg in patchAttr.NamedArguments)
            if (namedArg.Key == "TargetEntityType" && namedArg.Value.Value is INamedTypeSymbol typeSymbol)
                return typeSymbol;

        return null;
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
}
