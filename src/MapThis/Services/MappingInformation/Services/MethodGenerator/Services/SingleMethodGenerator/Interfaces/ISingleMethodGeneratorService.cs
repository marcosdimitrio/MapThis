using MapThis.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerator.Interfaces
{
    public interface ISingleMethodGeneratorService
    {
        MethodDeclarationSyntax Generate(MapInformationDto mapInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
