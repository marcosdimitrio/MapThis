using MapThis.CommonServices.AccessModifierIdentifiers.Interfaces;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace MapThis.Services.MappingInformation.MethodConstructors
{
    [Export(typeof(IRecursiveMethodConstructor))]
    public class RecursiveMethodConstructor : IRecursiveMethodConstructor
    {
        private readonly IList<IConstructor> _constructors;

        [ImportingConstructor]
        public RecursiveMethodConstructor(IMethodGeneratorFactory methodGeneratorFactory, IAccessModifierIdentifier accessModifierIdentifier)
        {
            _constructors = new List<IConstructor>()
            {
                new CollectionConstructor(this, methodGeneratorFactory, accessModifierIdentifier),
                new EnumConstructor(methodGeneratorFactory),
                new SimpleTypeConstructor(this, methodGeneratorFactory, accessModifierIdentifier),
                new PositionalRecordConstructor(this, methodGeneratorFactory, accessModifierIdentifier),
            };
        }

        public bool CanProcess(ITypeSymbol targetType, ITypeSymbol sourceType)
        {
            return _constructors.Any(x => x.CanProcess(targetType, sourceType));
        }

        public IMethodGenerator GetMap(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var targetType = currentMethodInformationDto.TargetType;
            var sourceType = currentMethodInformationDto.SourceType;

            if (targetType == null || sourceType == null)
            {
                return null;
            }

            foreach (var constructor in _constructors)
            {
                if (constructor.CanProcess(targetType, sourceType))
                {
                    return constructor.GetMap(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespaces);
                }
            }

            return null;
        }
    }
}
