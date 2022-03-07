using MapThis.Dto;
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

        public ClassMapGenerator(MapInformationDto mapInformationDto, ISingleMethodGeneratorService singleMethodGeneratorService)
        {
            SingleMethodGeneratorService = singleMethodGeneratorService;
            MapInformationDto = mapInformationDto;
        }

        public IList<MethodDeclarationSyntax> Generate()
        {
            var destination = new List<MethodDeclarationSyntax>() {
                SingleMethodGeneratorService.Generate(MapInformationDto)
            };

            foreach (var childCompoundGenerator in MapInformationDto.ChildrenCompoundGenerator)
            {
                destination.AddRange(childCompoundGenerator.Generate());
            }

            return destination;
        }

        public IList<INamespaceSymbol> GetNamespaces()
        {
            var namespaces = new List<INamespaceSymbol>()
            {
                MapInformationDto.SourceType.ContainingNamespace,
                MapInformationDto.TargetType.ContainingNamespace,
            };

            foreach (var childCompoundGenerator in MapInformationDto.ChildrenCompoundGenerator)
            {
                namespaces.AddRange(childCompoundGenerator.GetNamespaces());
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
