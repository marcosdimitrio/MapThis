using MapThis.CommonServices.AccessModifierIdentifiers.Interfaces;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords
{
    public class PositionalRecordConstructor : IConstructor
    {
        private readonly IRecursiveMethodConstructor RecursiveMethodConstructor;
        private readonly IMethodGeneratorFactory MethodGeneratorFactory;
        private readonly IAccessModifierIdentifier AccessModifierIdentifier;

        public PositionalRecordConstructor(IRecursiveMethodConstructor recursiveMethodConstructor, IMethodGeneratorFactory methodGeneratorFactory, IAccessModifierIdentifier accessModifierIdentifier)
        {
            RecursiveMethodConstructor = recursiveMethodConstructor;
            MethodGeneratorFactory = methodGeneratorFactory;
            AccessModifierIdentifier = accessModifierIdentifier;
        }

        public bool CanProcess(ITypeSymbol targetType, ITypeSymbol sourceType)
        {
            var canProcess =
                targetType.IsRecord &&
                sourceType.IsClass() &&
                HasExactlyOneExplicitDeclaredConstructor(targetType) &&
                AllPropertiesComeFromConstructor(targetType);

            return canProcess;
        }

        public IMethodGenerator GetMap(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var childrenMethodGenerators = new List<IMethodGenerator>();

            var propertiesToMap = new List<PropertyToMapDto>();

            var sourceMembers = currentMethodInformationDto.SourceType.GetPublicProperties();
            var targetMembers = currentMethodInformationDto.TargetType.GetPublicProperties();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInPropertySymbols(targetProperty, sourceMembers);

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, currentMethodInformationDto.FirstParameterName);

                propertiesToMap.Add(propertyToMap);

                var targetNamedType = targetProperty.Type as INamedTypeSymbol;
                var sourceNamedType = sourceProperty?.Type as INamedTypeSymbol;

                if (targetNamedType == null || sourceNamedType == null)
                {
                    continue;
                }

                var privateAccessModifiers = AccessModifierIdentifier.GetNewMethodAccessModifiers(currentMethodInformationDto.AccessModifiers);

                var firstParameterName = sourceNamedType.IsCollection() ? "source" : "item";

                var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceNamedType, targetNamedType, firstParameterName, new List<IParameterSymbol>());
                var childMethodGenerator = RecursiveMethodConstructor.GetMap(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                if (childMethodGenerator != null)
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }
            }

            var mapInformationForPositionalRecordDto = new MapInformationForPositionalRecordDto(currentMethodInformationDto, propertiesToMap, childrenMethodGenerators, optionsDto);

            var methodGenerator = MethodGeneratorFactory.Get(mapInformationForPositionalRecordDto, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

        private static bool HasExactlyOneExplicitDeclaredConstructor(ITypeSymbol targetType)
        {
            return ((INamedTypeSymbol)targetType).Constructors.Count(x => !x.IsImplicitlyDeclared) == 1;
        }

        private bool AllPropertiesComeFromConstructor(ITypeSymbol targetType)
        {
            var constructor = ((INamedTypeSymbol)targetType).Constructors.SingleOrDefault(x => !x.IsImplicitlyDeclared);

            if (constructor == null) return false;

            var allProperties = targetType.GetPublicProperties();
            var constructorParameters = constructor.Parameters.ToList();

            var allPropertiesComeFromConstructor = allProperties.All(x => constructorParameters.Any(y => x.MetadataName == y.MetadataName));

            return allPropertiesComeFromConstructor;
        }

        private static IPropertySymbol FindCorrespondingPropertyInPropertySymbols(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

    }
}
