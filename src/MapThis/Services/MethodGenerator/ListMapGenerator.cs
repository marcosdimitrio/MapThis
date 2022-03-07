using MapThis.Dto;
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

        public ListMapGenerator(MapCollectionInformationDto mapCollectionInformationDto, ISingleMethodGeneratorService singleMethodGeneratorService)
        {
            MapCollectionInformationDto = mapCollectionInformationDto;
            SingleMethodGeneratorService = singleMethodGeneratorService;
        }

        public IList<MethodDeclarationSyntax> Generate()
        {
            var destination = new List<MethodDeclarationSyntax>()
            {
                SingleMethodGeneratorService.Generate(MapCollectionInformationDto)
            };

            if (MapCollectionInformationDto.ChildCompoundGenerator != null)
            {
                destination.AddRange(MapCollectionInformationDto.ChildCompoundGenerator.Generate());
            }

            return destination;
        }

        public IList<INamespaceSymbol> GetNamespaces()
        {
            var namespaces = new List<INamespaceSymbol>()
            {
                MapCollectionInformationDto.SourceType.ContainingNamespace,
                MapCollectionInformationDto.TargetType.ContainingNamespace,
            };

            if (MapCollectionInformationDto.ChildCompoundGenerator != null)
            {
                namespaces.AddRange(MapCollectionInformationDto.ChildCompoundGenerator.GetNamespaces());
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
