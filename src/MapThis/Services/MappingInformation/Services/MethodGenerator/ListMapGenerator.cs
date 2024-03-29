﻿using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Refactorings.MappingRefactors.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerators.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator
{
    public class ListMapGenerator : IMethodGenerator
    {
        private readonly MapInformationForCollectionDto MapCollectionInformationDto;
        private readonly ICollectionMethodGenerator CollectionMethodGeneratorService;
        private readonly CodeAnalysisDependenciesDto CodeAnalisysDependenciesDto;
        private readonly IList<string> ExistingNamespaces;

        public ListMapGenerator(MapInformationForCollectionDto mapCollectionInformationDto, ICollectionMethodGenerator collectionMethodGeneratorService, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            MapCollectionInformationDto = mapCollectionInformationDto;
            CollectionMethodGeneratorService = collectionMethodGeneratorService;
            CodeAnalisysDependenciesDto = codeAnalisysDependenciesDto;
            ExistingNamespaces = existingNamespaces;
        }

        public GeneratedMethodsDto Generate()
        {
            var generatedMethodsDto = new GeneratedMethodsDto()
            {
                Blocks = GenerateBlocks(),
                Namespaces = GetNamespaces(),
            };

            return generatedMethodsDto;
        }

        private IList<MethodDeclarationSyntax> GenerateBlocks()
        {
            var destination = new List<MethodDeclarationSyntax>()
            {
                CollectionMethodGeneratorService.Generate(MapCollectionInformationDto, CodeAnalisysDependenciesDto, ExistingNamespaces)
            };

            if (MapCollectionInformationDto.ChildMethodGenerator != null)
            {
                destination.AddRange(MapCollectionInformationDto.ChildMethodGenerator.Generate().Blocks);
            }

            return destination;
        }

        private IList<string> GetNamespaces()
        {
            var namespaces = new List<INamespaceSymbol>();

            var sourceListType = MapCollectionInformationDto.MethodInformation.SourceType;
            var targetListType = MapCollectionInformationDto.MethodInformation.TargetType;
            var sourceType = MapCollectionInformationDto.MethodInformation.SourceType.GetElementType();
            var targetType = MapCollectionInformationDto.MethodInformation.TargetType.GetElementType();

            var sourceListNamespace = sourceListType.ContainingNamespace;
            var targetListNamespace = targetListType.ContainingNamespace;
            var sourceNamespace = sourceType.ContainingNamespace;
            var targetNamespace = targetType.ContainingNamespace;

            // namespace for the lists can be null when type is array
            if (!ExistingNamespaces.Contains(sourceListNamespace?.ToDisplayString()))
            {
                namespaces.Add(sourceListNamespace);
            }
            if (!ExistingNamespaces.Contains(targetListNamespace?.ToDisplayString()))
            {
                namespaces.Add(targetListNamespace);
            }
            if (!ExistingNamespaces.Contains(sourceNamespace.ToDisplayString()) && !sourceType.IsSimpleTypeWithAlias())
            {
                namespaces.Add(sourceNamespace);
            }
            if (!ExistingNamespaces.Contains(targetNamespace.ToDisplayString()) && !targetType.IsSimpleTypeWithAlias())
            {
                namespaces.Add(targetNamespace);
            }

            var namespacesString = namespaces
                .Where(x => x != null && !x.IsGlobalNamespace)
                .Select(x => x.ToDisplayString())
                .ToList();

            if (MapCollectionInformationDto.ChildMethodGenerator != null)
            {
                namespacesString.AddRange(MapCollectionInformationDto.ChildMethodGenerator.Generate().Namespaces);
            }

            if (sourceListType.IsArray())
            {
                namespacesString.Add("System.Collections.Generic");
            }

            namespacesString = namespacesString
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToList();

            return namespacesString;
        }

    }
}
