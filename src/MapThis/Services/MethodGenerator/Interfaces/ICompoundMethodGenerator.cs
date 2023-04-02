using MapThis.Refactorings.MappingGenerator.Dto;

namespace MapThis.Services.MethodGenerator.Interfaces
{
    public interface ICompoundMethodGenerator
    {
        GeneratedMethodsDto Generate();
    }
}
