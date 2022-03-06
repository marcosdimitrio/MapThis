using MapThis.Dto;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace MapThis.Services.MappingInformation.Interfaces
{
    public interface IMappingInformationService
    {
        Task<MapInformationDto> GetMapInformation(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken);
    }
}
