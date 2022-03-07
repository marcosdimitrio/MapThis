using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;

namespace MapThis.Services.MethodGenerator.Factories.Interfaces
{
    public interface ICompoundMethodGeneratorFactory
    {
        ICompoundMethodGenerator Get(MapInformationDto dto);
        ICompoundMethodGenerator Get(MapCollectionInformationDto dto);
    }
}
