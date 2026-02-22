using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

namespace FluentPatcher.Generator;

/// <summary>
/// Source generator that generates corresponding PatchContext and Patcher classes.
/// </summary>
[Generator]
internal sealed class FluentPatcherGenerator : IIncrementalGenerator
{
    private const string PatchForAttributeFullName = "FluentPatcher.Attributes.PatchForAttribute";
    private const string PatchForAttributeShortName = "PatchFor";
    private const string PatchForAttributeShortNameFull = "PatchForAttribute";

    /// <summary>
    /// Initializes the source generator by setting up the syntax provider to find candidate classes and register the source output for generation.
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all class declarations with potential [PatchFor] attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsCandidateForGeneration(node),
                transform: static (ctx, _) => GetSemanticTarget(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider
            .Combine(classDeclarations.Collect());

        // Generate the source
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateForGeneration(SyntaxNode node) =>
        // Quick syntax check: must be a class with at least one attribute
        node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTarget(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
            
        // Check if any attribute matches our marker
        foreach (var attributeListSyntax in classDecl.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);
                if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                var attributeContainingType = attributeSymbol.ContainingType;
                var fullName = attributeContainingType.ToDisplayString();
                    
                if (fullName == PatchForAttributeFullName)
                    return classDecl;
                    
                // Also check by short name for cases where using is present
                var name = attributeContainingType.Name;
                if (name == PatchForAttributeShortName || name == PatchForAttributeShortNameFull)
                    return classDecl;
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classes,
        SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        // Get unique classes
        var distinctClasses = classes.Distinct();

        foreach (var classDecl in distinctClasses)
        {
            var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

            if (classSymbol is null)
                continue;

            try
            {
                var model = ModelExtractor.ExtractModel(classSymbol);
                    
                // Generate PatchContext class
                var contextCode = ContextGenerator.Generate(model);
                context.AddSource($"{model.ClassName}PatchContext.g.cs", contextCode);
                    
                // Generate Patcher class
                var patcherCode = PatcherClassGenerator.Generate(model);
                context.AddSource($"{model.PatcherClassName}.g.cs", patcherCode);
            }
            catch (Exception)
            {
                // Ignored.
            }
        }
    }
}