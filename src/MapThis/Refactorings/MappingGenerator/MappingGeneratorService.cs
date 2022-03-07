using MapThis.Dto;
using MapThis.Refactorings.MappingGenerator.Interfaces;
using MapThis.Services.MappingInformation.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        public async Task<Document> ReplaceAsync(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var compilationUnitSyntax = (CompilationUnitSyntax)root;

            var compoundGenerator = await MappingInformationService.GetCompoundGenerators(context, methodSyntax, cancellationToken).ConfigureAwait(false);

            var namespaces = compoundGenerator.GetNamespaces();

            var blocks = compoundGenerator.Generate();

            var firstBlock = blocks.First();
            var allOtherBlocks = blocks.Skip(1).ToList();

            compilationUnitSyntax = compilationUnitSyntax.ReplaceNode(methodSyntax, methodSyntax.WithBody(firstBlock.Body));

            if (allOtherBlocks.Count > 0)
            {
                var listOfModifiers = new List<SyntaxKind>()
                {
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.ProtectedKeyword,
                    SyntaxKind.InternalKeyword,
                };

                var methodToInsertAfter = compilationUnitSyntax
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(x => x.Modifiers.Any(y => listOfModifiers.Contains(y.Kind())))
                    .LastOrDefault();

                methodToInsertAfter = methodToInsertAfter ?? (MethodDeclarationSyntax)compilationUnitSyntax.FindNode(context.Span);

                compilationUnitSyntax = compilationUnitSyntax.InsertNodesAfter(methodToInsertAfter, allOtherBlocks);
            }

            compilationUnitSyntax = await AddUsings(context, methodSyntax, compilationUnitSyntax, namespaces, cancellationToken).ConfigureAwait(false);

            return context.Document.WithSyntaxRoot(compilationUnitSyntax);
}

        private async Task<CompilationUnitSyntax> AddUsings(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CompilationUnitSyntax compilationUnitSyntax, IList<INamespaceSymbol> namespaces, CancellationToken cancellationToken)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken);
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
