﻿using MapThis.CommonServices.AccessModifierIdentifiers.Interfaces;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes
{
    public class SimpleTypeConstructor : IConstructor
    {
        private readonly IRecursiveMethodConstructor RecursiveMethodConstructor;
        private readonly IMethodGeneratorFactory MethodGeneratorFactory;
        private readonly IAccessModifierIdentifier AccessModifierIdentifier;

        public SimpleTypeConstructor(IRecursiveMethodConstructor methodConstructor, IMethodGeneratorFactory methodGeneratorFactory, IAccessModifierIdentifier accessModifierIdentifier)
        {
            RecursiveMethodConstructor = methodConstructor;
            MethodGeneratorFactory = methodGeneratorFactory;
            AccessModifierIdentifier = accessModifierIdentifier;
        }

        public bool CanProcess(ITypeSymbol targetType, ITypeSymbol sourceType)
        {
            var canProcess = BothTypesAreClasses(targetType, sourceType) && IsNotPositionalRecord(targetType);

            return canProcess;
        }

        // todo: almost exactly like PositionalRecordConstructor, refactor?
        public IMethodGenerator GetMap(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var childrenMethodGenerators = new List<IMethodGenerator>();

            var propertiesToMap = new List<PropertyToMapDto>();

            var sourceMembers = GetPublicPropertiesRecursively(currentMethodInformationDto.SourceType);
            var targetMembers = GetPublicPropertiesRecursively(currentMethodInformationDto.TargetType);

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

            var mapInformation = new MapInformationDto(currentMethodInformationDto, propertiesToMap, childrenMethodGenerators, optionsDto);

            var methodGenerator = MethodGeneratorFactory.Get(mapInformation, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

        private static bool BothTypesAreClasses(ITypeSymbol targetType, ITypeSymbol sourceType)
        {
            return targetType.IsClass() && sourceType.IsClass();
        }

        private static bool IsNotPositionalRecord(ITypeSymbol targetType)
        {
            var hasExplicitConstructors = ((INamedTypeSymbol)targetType).Constructors.Where(x => !x.IsImplicitlyDeclared).Any();

            return !(targetType.IsRecord && hasExplicitConstructors);
        }

        private static IPropertySymbol FindCorrespondingPropertyInPropertySymbols(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        /// <summary>
        /// Note: The properties from base classes will be listed first.
        /// </summary>
        private static IList<IPropertySymbol> GetPublicPropertiesRecursively(ITypeSymbol sourceType)
        {
            var properties = sourceType.GetPublicProperties().ToList();

            if (sourceType.BaseType.GetPublicProperties().Count > 0)
            {
                var baseTypeProperties = GetPublicPropertiesRecursively(sourceType.BaseType);
                properties = baseTypeProperties.Concat(properties).ToList();
            }

            return properties;
        }

    }
}
