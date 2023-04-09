using MapThis.Refactorings.MappingRefactors.Dto;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces
{
    public interface IMethodGenerator
    {
        GeneratedMethodsDto Generate();
    }
}
