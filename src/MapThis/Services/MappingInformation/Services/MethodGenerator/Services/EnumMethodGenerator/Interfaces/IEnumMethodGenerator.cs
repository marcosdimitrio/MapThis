using MapThis.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerator.Interfaces
{
    public interface IEnumMethodGenerator
    {
        MethodDeclarationSyntax Generate(MapEnumInformationDto mapEnumInformationDto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
