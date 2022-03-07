using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.CompoundGenerator
{
    public class ClassMapGenerator : ICompoundGenerator
    {
        private readonly MapInformationDto MapInformationDto;
        private readonly IMethodGeneratorService MethodGeneratorService;

        public ClassMapGenerator(MapInformationDto mapInformationDto, IMethodGeneratorService methodGeneratorService)
        {
            MethodGeneratorService = methodGeneratorService;
            MapInformationDto = mapInformationDto;
        }

        public IList<MethodDeclarationSyntax> Generate()
        {
            var methodDeclarationSyntax = MethodGeneratorService.Generate(MapInformationDto);

            var destination = new List<MethodDeclarationSyntax>() { methodDeclarationSyntax };

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
