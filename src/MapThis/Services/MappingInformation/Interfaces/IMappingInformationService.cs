using MapThis.Services.CompoundGenerator.Interfaces;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using System.Threading.Tasks;

namespace MapThis.Services.MappingInformation.Interfaces
{
    public interface IMappingInformationService
    {
        Task<ICompoundMethodGenerator> GetCompoundMethodsGenerator(Dto.OptionsDto optionsDto, CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken);
    }
}
