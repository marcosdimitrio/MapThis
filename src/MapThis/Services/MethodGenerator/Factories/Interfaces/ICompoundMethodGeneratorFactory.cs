using MapThis.Dto;
using MapThis.Services.MethodGenerator.Interfaces;
using System.Collections.Generic;

namespace MapThis.Services.MethodGenerator.Factories.Interfaces
{
    public interface ICompoundMethodGeneratorFactory
    {
        ICompoundMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
        ICompoundMethodGenerator Get(MapCollectionInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
        ICompoundMethodGenerator Get(MapEnumInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces);
    }
}
