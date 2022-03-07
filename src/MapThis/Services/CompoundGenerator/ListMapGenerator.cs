using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.CompoundGenerator
{
    public class ListMapGenerator : ICompoundGenerator
    {
        private readonly MapCollectionInformationDto MapCollectionInformationDto;
        private readonly IMethodGeneratorService MethodGeneratorService;

        public ListMapGenerator(MapCollectionInformationDto mapCollectionInformationDto, IMethodGeneratorService methodGeneratorService)
        {
            MapCollectionInformationDto = mapCollectionInformationDto;
            MethodGeneratorService = methodGeneratorService;
        }

        public IList<MethodDeclarationSyntax> Generate()
        {
            var destination = new List<MethodDeclarationSyntax>();

            destination.Add(MethodGeneratorService.Generate(MapCollectionInformationDto));

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
                .Where(x => !x.IsGlobalNamespace)
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x.ToDisplayString())
                .ToList();

            return namespaces;
        }

    }
}
