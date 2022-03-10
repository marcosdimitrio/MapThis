using MapThis.Dto;
using MapThis.Refactorings.MappingGenerator.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.SingleMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.CompoundGenerator
{
    public class ClassMapGenerator : ICompoundMethodGenerator
    {
        private readonly MapInformationDto MapInformationDto;
        private readonly ISingleMethodGeneratorService SingleMethodGeneratorService;
        private readonly CodeAnalysisDependenciesDto CodeAnalisysDependenciesDto;

        public ClassMapGenerator(MapInformationDto mapInformationDto, ISingleMethodGeneratorService singleMethodGeneratorService, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            SingleMethodGeneratorService = singleMethodGeneratorService;
            MapInformationDto = mapInformationDto;
            CodeAnalisysDependenciesDto = codeAnalisysDependenciesDto;
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
            var destination = new List<MethodDeclarationSyntax>() {
                SingleMethodGeneratorService.Generate(MapInformationDto, CodeAnalisysDependenciesDto)
            };

            foreach (var childCompoundGenerator in MapInformationDto.ChildrenMethodGenerators)
            {
                destination.AddRange(childCompoundGenerator.Generate().Blocks);
            }

            return destination;
        }

        private IList<INamespaceSymbol> GetNamespaces()
        {
            var namespaces = new List<INamespaceSymbol>()
            {
                MapInformationDto.SourceType.ContainingNamespace,
                MapInformationDto.TargetType.ContainingNamespace,
            };

            foreach (var childCompoundGenerator in MapInformationDto.ChildrenMethodGenerators)
            {
                namespaces.AddRange(childCompoundGenerator.Generate().Namespaces);
            }

            namespaces = namespaces
                .Where(x => !x.IsGlobalNamespace)
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x.ToDisplayString())
                .ToList();

            return namespaces;
        }

    }
}
