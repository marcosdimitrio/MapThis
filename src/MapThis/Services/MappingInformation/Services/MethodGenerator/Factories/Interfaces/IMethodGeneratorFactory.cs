using MapThis.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords.Dto;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using System.Collections.Generic;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces
{
    public interface IMethodGeneratorFactory
    {
        IMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
        IMethodGenerator Get(MapInformationForCollectionDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
        IMethodGenerator Get(MapEnumInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
        IMethodGenerator Get(MapInformationForPositionalRecordDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
