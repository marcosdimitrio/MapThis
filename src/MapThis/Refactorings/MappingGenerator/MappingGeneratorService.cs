using MapThis.Dto;
using MapThis.Refactorings.MappingGenerator.Interfaces;
using MapThis.Services.MappingInformation.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Refactorings.MappingGenerator
{
    [Export(typeof(IMappingGeneratorService))]
    public class MappingGeneratorService : IMappingGeneratorService
    {
        private readonly IMappingInformationService MappingInformationService;

        [ImportingConstructor]
        public MappingGeneratorService(IMappingInformationService mappingInformationService)
        {
            MappingInformationService = mappingInformationService;
        }

        public async Task<Document> ReplaceAsync(OptionsDto optionsDto, CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var compilationUnitSyntax = (CompilationUnitSyntax)root;
            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var originalMethodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken);
            var codeAnalisysDependenciesDto = new CodeAnalysisDependenciesDto()
            {
                SyntaxGenerator = SyntaxGenerator.GetGenerator(context.Document),
            };

            var compoundMethodsGenerator = MappingInformationService.GetCompoundMethodsGenerator(optionsDto, methodSyntax, originalMethodSymbol, root, semanticModel, codeAnalisysDependenciesDto);

            var generatedMethodsDto = compoundMethodsGenerator.Generate();

            var firstBlock = generatedMethodsDto.Blocks.First();
            var allOtherBlocks = generatedMethodsDto.Blocks.Skip(1).ToList();

            var firstBlockMethodSyntaxFixed = GetFirstBlockWithOriginalSignature(methodSyntax, firstBlock.Body);

            compilationUnitSyntax = compilationUnitSyntax.ReplaceNode(methodSyntax, firstBlockMethodSyntaxFixed);

            if (allOtherBlocks.Count > 0)
            {
                var methodToInsertAfter = GetMethodToInsertAfter(context, compilationUnitSyntax);

                compilationUnitSyntax = compilationUnitSyntax.InsertNodesAfter(methodToInsertAfter, allOtherBlocks);
            }

            compilationUnitSyntax = AddMissingUsings(originalMethodSymbol, compilationUnitSyntax, generatedMethodsDto.Namespaces);

            return context.Document.WithSyntaxRoot(compilationUnitSyntax);
        }

        private static MethodDeclarationSyntax GetMethodToInsertAfter(CodeRefactoringContext context, CompilationUnitSyntax compilationUnitSyntax)
        {
            var listOfModifiers = new List<SyntaxKind>()
            {
                SyntaxKind.PublicKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword,
            };

            var lastNonPrivateMethod = compilationUnitSyntax
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(x => x.Modifiers.Any(y => listOfModifiers.Contains(y.Kind())))
                .LastOrDefault();

            var methodToInsertAfter = (MethodDeclarationSyntax)compilationUnitSyntax.FindNode(context.Span);

            if (lastNonPrivateMethod != null && lastNonPrivateMethod.Span.End > methodToInsertAfter.Span.End)
            {
                methodToInsertAfter = lastNonPrivateMethod;
            }

            return methodToInsertAfter;
        }

        private MethodDeclarationSyntax GetFirstBlockWithOriginalSignature(MethodDeclarationSyntax methodSyntax, BlockSyntax newBodyBlock)
        {
            // Recreating the MethodDeclaration will fix when the method's body is null,
            // i.e., when it doesn't have opening/closing braces "{}".
            var methodDeclarationSyntax =
                MethodDeclaration(
                    methodSyntax.AttributeLists,
                    methodSyntax.Modifiers,
                    methodSyntax.ReturnType,
                    methodSyntax.ExplicitInterfaceSpecifier,
                    methodSyntax.Identifier,
                    methodSyntax.TypeParameterList,
                    methodSyntax.ParameterList,
                    methodSyntax.ConstraintClauses,
                    newBodyBlock,
                    methodSyntax.SemicolonToken
                )
                .WithTrailingTrivia(EndOfLine(Environment.NewLine));

            return methodDeclarationSyntax;
        }

        private CompilationUnitSyntax AddMissingUsings(IMethodSymbol methodSymbol, CompilationUnitSyntax compilationUnitSyntax, IList<INamespaceSymbol> namespaces)
        {
            var currentNamespace = methodSymbol.ContainingNamespace;

            foreach (var namespaceToInclude in namespaces)
            {
                var usingAlreadyExists = compilationUnitSyntax.Usings.Any(x => x.Name.ToFullString() == namespaceToInclude.ToDisplayString());

                var namespaceIsTheSameAsTheMethod = SymbolEqualityComparer.Default.Equals(currentNamespace, namespaceToInclude);

                var currentNamespaceIsDeeperThanBeingMapped = currentNamespace.ToDisplayString().StartsWith(namespaceToInclude.ToDisplayString());

                if (!usingAlreadyExists && !namespaceIsTheSameAsTheMethod && !currentNamespaceIsDeeperThanBeingMapped)
                {
                    compilationUnitSyntax = compilationUnitSyntax
                        .AddUsings(UsingDirective(IdentifierName(namespaceToInclude.ToDisplayString())));
                }
            }

            return compilationUnitSyntax;
        }

    }
}
