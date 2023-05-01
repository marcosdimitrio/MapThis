using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerators.Interfaces
{
    public interface ICollectionMethodGenerator
    {
        MethodDeclarationSyntax Generate(MapInformationForCollectionDto childMapCollectionInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
