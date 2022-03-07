using MapThis.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapThis.Services.SingleMethodGenerator.Interfaces
{
    public interface ISingleMethodGeneratorService
    {
        MethodDeclarationSyntax Generate(MapInformationDto mapInformation);
        MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation);
    }
}
