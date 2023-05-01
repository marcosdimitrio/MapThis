using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums
{
    public class EnumConstructor : IConstructor
    {
        private readonly IMethodGeneratorFactory _methodGeneratorFactory;

        public EnumConstructor(IMethodGeneratorFactory methodGeneratorFactory)
        {
            _methodGeneratorFactory = methodGeneratorFactory;
        }

        public bool CanProcess(ITypeSymbol targetType, ITypeSymbol sourceType)
        {
            var canProcess = targetType.IsEnum() && sourceType.IsEnum();

            return canProcess;
        }

        public IMethodGenerator GetMap(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var enumItemsToMap = new List<EnumItemToMapDto>();

            var sourceMembers = currentMethodInformationDto.SourceType.GetMembers().Where(x => x.Kind == SymbolKind.Field).ToList();
            var targetMembers = currentMethodInformationDto.TargetType.GetMembers().Where(x => x.Kind == SymbolKind.Field).ToList();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInSymbols(targetProperty, sourceMembers);

                var enumItemToMap = new EnumItemToMapDto(targetProperty, currentMethodInformationDto.FirstParameterName);

                enumItemsToMap.Add(enumItemToMap);
            }

            var mapInformation = new MapEnumInformationDto(currentMethodInformationDto, enumItemsToMap, optionsDto);

            var methodGenerator = _methodGeneratorFactory.Get(mapInformation, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

        private static ISymbol FindCorrespondingPropertyInSymbols(ISymbol targetProperty, IList<ISymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

    }
}
