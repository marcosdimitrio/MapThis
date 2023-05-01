using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.PositionalRecordMethodGenerators.Interfaces
{
    public interface IPositionalRecordMethodGenerator
    {
        MethodDeclarationSyntax Generate(MapInformationForPositionalRecordDto mapInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
