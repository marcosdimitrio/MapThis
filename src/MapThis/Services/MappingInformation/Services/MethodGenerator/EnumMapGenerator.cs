using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Refactorings.MappingRefactors.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator
{
    public class EnumMapGenerator : IMethodGenerator
    {
        private readonly MapEnumInformationDto MapEnumInformationDto;
        private readonly IEnumMethodGenerator EnumMethodGenerator;
        private readonly CodeAnalysisDependenciesDto CodeAnalisysDependenciesDto;
        private readonly IList<string> ExistingNamespaces;

        public EnumMapGenerator(MapEnumInformationDto mapEnumInformationDto, IEnumMethodGenerator enumMethodGenerator, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            MapEnumInformationDto = mapEnumInformationDto;
            EnumMethodGenerator = enumMethodGenerator;
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
            var destination = new List<MethodDeclarationSyntax>() {
                EnumMethodGenerator.Generate(MapEnumInformationDto, CodeAnalisysDependenciesDto, ExistingNamespaces)
            };

            return destination;
        }

        private IList<string> GetNamespaces()
        {
            var namespaces = new List<INamespaceSymbol>();

            var sourceType = MapEnumInformationDto.MethodInformation.SourceType;
            var targetType = MapEnumInformationDto.MethodInformation.TargetType;

            var sourceNamespace = sourceType.ContainingNamespace;
            var targetNamespace = targetType.ContainingNamespace;

            if (!ExistingNamespaces.Contains(sourceNamespace.ToDisplayString()) && !sourceType.IsSimpleTypeWithAlias())
            {
                namespaces.Add(sourceNamespace);
            }
            if (!ExistingNamespaces.Contains(targetNamespace.ToDisplayString()) && !targetType.IsSimpleTypeWithAlias())
            {
                namespaces.Add(targetNamespace);
            }

            var namespaceStringList = namespaces
                .Where(x => x != null && !x.IsGlobalNamespace)
                .Select(x => x.ToDisplayString())
                .ToList();

            // Necessary for InvalidEnumArgumentException which is thrown in the switch case's default option
            if (!ExistingNamespaces.Contains("System.ComponentModel"))
            {
                namespaceStringList.Add("System.ComponentModel");
            }

            namespaceStringList = namespaceStringList
                .GroupBy(x => x)
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToList();

            return namespaceStringList;
        }
    }
}
