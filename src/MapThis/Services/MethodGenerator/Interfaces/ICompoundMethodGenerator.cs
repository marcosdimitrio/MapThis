using MapThis.Refactorings.MappingGenerator.Dto;

namespace MapThis.Services.CompoundGenerator.Interfaces
{
    public interface ICompoundMethodGenerator
    {
        GeneratedMethodsDto Generate();
    }
}
