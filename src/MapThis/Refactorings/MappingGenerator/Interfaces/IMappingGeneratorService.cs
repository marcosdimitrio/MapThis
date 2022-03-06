using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace MapThis.Refactorings.MappingGenerator.Interfaces
{
    public interface IMappingGeneratorService
    {
        Task<Document> ReplaceAsync(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken);
    }
}
