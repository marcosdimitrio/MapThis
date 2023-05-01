using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerators.Interfaces
{
    public interface ISingleMethodGenerator
    {
        MethodDeclarationSyntax Generate(MapInformationDto mapInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
