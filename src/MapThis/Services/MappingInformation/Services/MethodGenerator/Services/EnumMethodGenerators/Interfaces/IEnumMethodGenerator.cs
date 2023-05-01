using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerators.Interfaces
{
    public interface IEnumMethodGenerator
    {
        MethodDeclarationSyntax Generate(MapEnumInformationDto mapEnumInformationDto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
