using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;

namespace MapThis.Services.MethodGenerator.Factories.Interfaces
{
    public interface ICompoundMethodGeneratorFactory
    {
        ICompoundMethodGenerator Get(MapInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto);
        ICompoundMethodGenerator Get(MapCollectionInformationDto dto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto);
    }
}
