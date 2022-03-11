using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Refactorings.MappingGenerator.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.SingleMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.CompoundGenerator
{
    public class ListMapGenerator : ICompoundMethodGenerator
    {
        private readonly MapCollectionInformationDto MapCollectionInformationDto;
        private readonly ISingleMethodGeneratorService SingleMethodGeneratorService;
        private readonly CodeAnalysisDependenciesDto CodeAnalisysDependenciesDto;
        private readonly IList<string> ExistingNamespaces;

        public ListMapGenerator(MapCollectionInformationDto mapCollectionInformationDto, ISingleMethodGeneratorService singleMethodGeneratorService, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            MapCollectionInformationDto = mapCollectionInformationDto;
            SingleMethodGeneratorService = singleMethodGeneratorService;
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
                SingleMethodGeneratorService.Generate(MapCollectionInformationDto, CodeAnalisysDependenciesDto, ExistingNamespaces)
            };

            if (MapCollectionInformationDto.ChildMethodGenerator != null)
            {
                destination.AddRange(MapCollectionInformationDto.ChildMethodGenerator.Generate().Blocks);
            }

            return destination;
        }

        private IList<INamespaceSymbol> GetNamespaces()
        {
            var namespaces = new List<INamespaceSymbol>();

            var sourceNamespace = MapCollectionInformationDto.MethodInformation.SourceType.ContainingNamespace;
            var targetNamespace = MapCollectionInformationDto.MethodInformation.TargetType.ContainingNamespace;
            var sourceListNamespace = MapCollectionInformationDto.MethodInformation.SourceType.GetElementType().ContainingNamespace;
            var targetListNamespace = MapCollectionInformationDto.MethodInformation.TargetType.GetElementType().ContainingNamespace;

            if (!ExistingNamespaces.Contains(sourceNamespace.ToDisplayString()))
            {
                namespaces.Add(sourceNamespace);
            }
            if (!ExistingNamespaces.Contains(targetNamespace.ToDisplayString()))
            {
                namespaces.Add(targetNamespace);
            }
            if (!ExistingNamespaces.Contains(sourceListNamespace.ToDisplayString()))
            {
                namespaces.Add(sourceListNamespace);
            }
            if (!ExistingNamespaces.Contains(targetListNamespace.ToDisplayString()))
            {
                namespaces.Add(targetListNamespace);
            }

            if (MapCollectionInformationDto.ChildMethodGenerator != null)
            {
                namespaces.AddRange(MapCollectionInformationDto.ChildMethodGenerator.Generate().Namespaces);
            }

            namespaces = namespaces
                .Where(x => x != null && !x.IsGlobalNamespace)
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x.ToDisplayString())
                .ToList();

            return namespaces;
        }

    }
}
