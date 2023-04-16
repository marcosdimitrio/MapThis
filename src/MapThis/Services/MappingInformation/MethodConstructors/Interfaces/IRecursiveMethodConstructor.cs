using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.MethodConstructors.Interfaces
{
    public interface IRecursiveMethodConstructor
    {
        bool CanProcess(ITypeSymbol targetType, ITypeSymbol sourceType);
        IMethodGenerator GetMap(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces);
    }
}
