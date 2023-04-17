﻿using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerator.Interfaces
{
    public interface ICollectionMethodGeneratorService
    {
        MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
