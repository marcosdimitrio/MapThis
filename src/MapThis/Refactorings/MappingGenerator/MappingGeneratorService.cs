using MapThis.Dto;
using MapThis.Refactorings.MappingGenerator.Interfaces;
using MapThis.Services.MappingInformation.Interfaces;
using MapThis.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        private readonly IMethodGeneratorService MethodGeneratorService;
        private readonly IMappingInformationService MappingInformationService;

        [ImportingConstructor]
        public MappingGeneratorService(IMethodGeneratorService methodGeneratorService, IMappingInformationService mappingInformationService)
        {
            MethodGeneratorService = methodGeneratorService;
            MappingInformationService = mappingInformationService;
        }

        public async Task<Document> ReplaceAsync(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var compilationUnitSyntax = (CompilationUnitSyntax)root;

            var mapInformation = await MappingInformationService.GetMapInformation(context, methodSyntax, cancellationToken).ConfigureAwait(false);

            var blocks = GetBlocks(mapInformation);

            var firstBlock = blocks.First();
            var allOtherBlocks = blocks.Skip(1).ToList();

            compilationUnitSyntax = compilationUnitSyntax.ReplaceNode(methodSyntax, firstBlock);

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

            compilationUnitSyntax = await AddUsings(context, methodSyntax, compilationUnitSyntax, mapInformation, cancellationToken).ConfigureAwait(false);

            return context.Document.WithSyntaxRoot(compilationUnitSyntax);
        }

        private IList<MethodDeclarationSyntax> GetBlocks(MapInformationDto mapInformation)
        {
            var blocks = new List<MethodDeclarationSyntax>();

            var firstBlockSyntax = MethodGeneratorService.Generate(mapInformation);

            blocks.Add(firstBlockSyntax);

            foreach (var childMapCollectionInformation in mapInformation.ChildrenMapCollectionInformation)
            {
                var blockSyntax = MethodGeneratorService.Generate(childMapCollectionInformation);

                blocks.Add(blockSyntax);

                if (childMapCollectionInformation.ChildMapInformation != null)
                {
                    blocks.AddRange(GetBlocks(childMapCollectionInformation.ChildMapInformation));
                }
            }

            foreach (var childMapInformation in mapInformation.ChildrenMapInformation)
            {
                blocks.AddRange(GetBlocks(childMapInformation));
            }

            return blocks;
        }

        private async Task<CompilationUnitSyntax> AddUsings(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CompilationUnitSyntax compilationUnitSyntax, MapInformationDto mapInformation, CancellationToken cancellationToken)
        {
            var namespaces = GetNamespaces(mapInformation);

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

        private IList<INamespaceSymbol> GetNamespaces(MapInformationDto mapInformation)
        {
            var namespaces = new List<INamespaceSymbol>()
            {
                mapInformation.SourceType.ContainingNamespace,
                mapInformation.TargetType.ContainingNamespace,
            };

            foreach (var childMapCollectionInformation in mapInformation.ChildrenMapCollectionInformation)
            {
                namespaces.Add(childMapCollectionInformation.SourceType.ContainingNamespace);
                namespaces.Add(childMapCollectionInformation.TargetType.ContainingNamespace);

                if (childMapCollectionInformation.ChildMapInformation != null)
                {
                    namespaces.AddRange(GetNamespaces(childMapCollectionInformation.ChildMapInformation));
                }
            }

            foreach (var childMapInformation in mapInformation.ChildrenMapInformation)
            {
                namespaces.AddRange(GetNamespaces(childMapInformation));
            }

            namespaces = namespaces
                .Where(x => !x.IsGlobalNamespace)
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x.ToDisplayString())
                .ToList();

            return namespaces;
        }

    }
}
