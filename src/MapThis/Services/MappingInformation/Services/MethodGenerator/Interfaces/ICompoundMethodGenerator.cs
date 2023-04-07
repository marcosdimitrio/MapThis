using MapThis.Refactorings.MappingGenerator.Dto;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces
{
    public interface ICompoundMethodGenerator
    {
        GeneratedMethodsDto Generate();
    }
}
