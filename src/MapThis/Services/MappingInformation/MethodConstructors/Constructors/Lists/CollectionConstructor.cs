using MapThis.CommonServices.AccessModifierIdentifiers.Interfaces;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists
{
    public class CollectionConstructor : IConstructor
    {
        private readonly IRecursiveMethodConstructor RecursiveMethodConstructor;
        private readonly IMethodGeneratorFactory MethodGeneratorFactory;
        private readonly IAccessModifierIdentifier AccessModifierIdentifier;

        public CollectionConstructor(IRecursiveMethodConstructor methodConstructor, IMethodGeneratorFactory methodGeneratorFactory, IAccessModifierIdentifier accessModifierIdentifier)
        {
            RecursiveMethodConstructor = methodConstructor;
            MethodGeneratorFactory = methodGeneratorFactory;
            AccessModifierIdentifier = accessModifierIdentifier;
        }

        public bool CanProcess(ITypeSymbol targetType, ITypeSymbol sourceType)
        {
            return targetType.IsCollection() && sourceType.IsCollection();
        }

        public IMethodGenerator GetMap(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var sourceElementType = (INamedTypeSymbol)currentMethodInformationDto.SourceType.GetElementType();
            var targetElementType = (INamedTypeSymbol)currentMethodInformationDto.TargetType.GetElementType();

            var privateAccessModifiers = AccessModifierIdentifier.GetNewMethodAccessModifiers(currentMethodInformationDto.AccessModifiers);

            var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceElementType, targetElementType, "item", new List<IParameterSymbol>());

            IMethodGenerator childMethodGenerator = null;
            if (existingMethodsControlService.TryAddMethod(sourceElementType, targetElementType))
            {
                childMethodGenerator = RecursiveMethodConstructor.GetMap(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(currentMethodInformationDto, childMethodGenerator, optionsDto);

            var methodGenerator = MethodGeneratorFactory.Get(mapCollectionInformationDto, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

    }
}
