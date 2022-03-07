using MapThis.Refactorings.MappingGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Threading.Tasks;

//TODO: Map collections as the first item being mapped
//TODO: Mapping methods without brackets {} should fix the spacing between created methods
//TODO: Refactor
//TODO: Test with IEnumerable, ICollection
//TODO: Add option to check for null (if (item == null) return null;)
namespace MapThis
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MapThisCodeRefactoringProvider)), Shared]
    internal class MapThisCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly IMappingGeneratorService MappingGeneratorService;

        [ImportingConstructor]
        public MapThisCodeRefactoringProvider(IMappingGeneratorService mappingGeneratorService)
        {
            MappingGeneratorService = mappingGeneratorService;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            //root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());

            var methodDeclaration = node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
            {
                return;
            }
            if (methodDeclaration.Parent?.Kind() == SyntaxKind.InterfaceDeclaration)
            {
                return;
            }
            if (methodDeclaration.ParameterList.Parameters.Count == 0)
            {
                return;
            }

            Register(context, methodDeclaration);
        }

        private void Register(CodeRefactoringContext context, MethodDeclarationSyntax methodDeclaration)
        {
            var action = CodeAction.Create("Map this", c => MappingGeneratorService.ReplaceAsync(context, methodDeclaration, c));

            context.RegisterRefactoring(action);
        }

    }
}
