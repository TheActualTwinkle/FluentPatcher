using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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
                var propForNullable = node.AncestorsAndSelf().OfType<PropertyDeclarationSyntax>().FirstOrDefault();
                if (propForNullable != null)
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Make Patchable inner type nullable",
                            _ => Task.FromResult(MakePatchableInnerTypeNullable(context.Document, root, propForNullable)),
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

    private static Document MakePatchableInnerTypeNullable(
        Document document,
        SyntaxNode root,
        PropertyDeclarationSyntax property)
    {
        var originalType = property.Type;

        // If Patchable<T>
        if (originalType is GenericNameSyntax genericName &&
            genericName.Identifier.Text == "Patchable" &&
            genericName.TypeArgumentList.Arguments.Count == 1)
        {
            var innerType = genericName.TypeArgumentList.Arguments[0];

            // If already nullable, do nothing
            if (innerType is NullableTypeSyntax)
                return document;

            // If reference type and not nullable, add ?
            TypeSyntax newInnerType;
            if (innerType is PredefinedTypeSyntax || innerType is IdentifierNameSyntax || innerType is QualifiedNameSyntax || innerType is GenericNameSyntax)
            {
                newInnerType = SyntaxFactory.NullableType(innerType.WithoutTrivia());
            }
            else
            {
                newInnerType = SyntaxFactory.NullableType(innerType.WithoutTrivia());
            }

            var newGeneric = genericName.WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(newInnerType)
                )
            ).WithTriviaFrom(genericName);

            var newProperty = property.WithType(newGeneric);
            var newRoot = root.ReplaceNode(property, newProperty);
            return document.WithSyntaxRoot(newRoot);
        }

        // If not Patchable<T>, just make type nullable
        if (originalType is PredefinedTypeSyntax || originalType is IdentifierNameSyntax || originalType is QualifiedNameSyntax || originalType is GenericNameSyntax)
        {
            // If already nullable, do nothing
            if (originalType is NullableTypeSyntax)
                return document;

            var newType = SyntaxFactory.NullableType(originalType.WithoutTrivia()).WithTriviaFrom(originalType);
            var newProperty = property.WithType(newType);
            var newRoot = root.ReplaceNode(property, newProperty);
            return document.WithSyntaxRoot(newRoot);
        }

        // Otherwise, do nothing
        return document;
    }

}