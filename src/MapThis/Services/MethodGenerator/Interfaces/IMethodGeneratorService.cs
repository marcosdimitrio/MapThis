using MapThis.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapThis.Services.MethodGenerator.Interfaces
{
    public interface IMethodGeneratorService
    {
        MethodDeclarationSyntax Generate(MapInformationDto mapInformation);
        MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation);
    }
}
