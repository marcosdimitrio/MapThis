using MapThis.Dto;
using MapThis.Services.CompoundGenerator.Interfaces;

namespace MapThis.Services.CompoundGenerator.Factories.Interfaces
{
    public interface ICompoundGeneratorFactory
    {
        ICompoundGenerator Get(MapInformationDto dto);
        ICompoundGenerator Get(MapCollectionInformationDto dto);
    }
}
