using MapThis.Dto;
using MapThis.Refactorings.MappingRefactors.Interfaces;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace MapThis
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MapThisCodeRefactoringProvider)), Shared]
    public class MapThisCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly IMappingRefactorService MappingRefactorService;
        private readonly IRecursiveMethodConstructor RecursiveMethodConstructor;

        [ImportingConstructor]
        public MapThisCodeRefactoringProvider(IMappingRefactorService mappingRefactorService, IRecursiveMethodConstructor recursiveMethodConstructor)
        {
            MappingRefactorService = mappingRefactorService;
            RecursiveMethodConstructor = recursiveMethodConstructor;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);

            var methodDeclaration = FindMethodDeclaration(node);

            if (methodDeclaration == null)
            {
                return;
            }
            if (methodDeclaration.Parent?.IsKind(SyntaxKind.InterfaceDeclaration) ?? false)
            {
                return;
            }
            if (methodDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
            {
                return;
            }
            if (methodDeclaration.ParameterList.Parameters.Count == 0)
            {
                return;
            }
            if (methodDeclaration.ReturnType is PredefinedTypeSyntax returnTypeSyntax)
            {
                if (returnTypeSyntax.Keyword.IsKind(SyntaxKind.VoidKeyword))
                {
                    return;
                }
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            if (methodSymbol.ReturnType.TypeKind == TypeKind.Error || methodSymbol.Parameters.First().Type.TypeKind == TypeKind.Error)
            {
                return;
            }

            if (!RecursiveMethodConstructor.CanProcess(methodSymbol.ReturnType, methodSymbol.Parameters.First().Type))
            {
                return;
            }

            Register(context, methodDeclaration);
        }

        private static MethodDeclarationSyntax FindMethodDeclaration(SyntaxNode node)
        {
            if (node is MethodDeclarationSyntax methodDeclaration)
            {
                return methodDeclaration;
            }

            if (node?.Parent is MethodDeclarationSyntax parent)
            {
                return parent;
            }

            return null;
        }

        private void Register(CodeRefactoringContext context, MethodDeclarationSyntax methodDeclaration)
        {
            var options1 = new OptionsDto() { NullChecking = false };
            var options2 = new OptionsDto() { NullChecking = true };

            var action1 = CodeAction.Create("Map this", c => MappingRefactorService.ReplaceAsync(options1, context, methodDeclaration, c));
            var action2 = CodeAction.Create("Map this with null check", c => MappingRefactorService.ReplaceAsync(options2, context, methodDeclaration, c));

            context.RegisterRefactoring(action1);
            context.RegisterRefactoring(action2);
        }

    }
}
