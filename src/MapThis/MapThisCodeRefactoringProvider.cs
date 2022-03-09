using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Refactorings.MappingGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

//TODO: Map non nullable to nullable and vice-versa
//TODO: Fix mapping of classes that have type (DataTablesResponse<MyClass>)
//TODO: Map arrays
namespace MapThis
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MapThisCodeRefactoringProvider)), Shared]
    public class MapThisCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly IMappingGeneratorService MappingGeneratorService;

        [ImportingConstructor]
        public MapThisCodeRefactoringProvider(IMappingGeneratorService mappingGeneratorService)
        {
            MappingGeneratorService = mappingGeneratorService;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            //root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

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

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            if (methodSymbol.ReturnType.IsCollection() && !methodSymbol.Parameters.First().Type.IsCollection() ||
                !methodSymbol.ReturnType.IsCollection() && methodSymbol.Parameters.First().Type.IsCollection())
            {
                return;
            }

            Register(context, methodDeclaration);
        }

        private void Register(CodeRefactoringContext context, MethodDeclarationSyntax methodDeclaration)
        {
            var options1 = new OptionsDto() { NullChecking = false };
            var options2 = new OptionsDto() { NullChecking = true };

            var action1 = CodeAction.Create("Map this", c => MappingGeneratorService.ReplaceAsync(options1, context, methodDeclaration, c));
            var action2 = CodeAction.Create("Map this with null check", c => MappingGeneratorService.ReplaceAsync(options2, context, methodDeclaration, c));

            context.RegisterRefactoring(action1);
            context.RegisterRefactoring(action2);
        }

    }
}
