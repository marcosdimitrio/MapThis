using Microsoft.CodeAnalysis;

namespace MapThis.Services.MappingInformation.Services.ExistingMethodsControl.Interfaces
{
    public interface IExistingMethodsControlService
    {
        bool TryAddMethod(INamedTypeSymbol sourceType, INamedTypeSymbol targetType);
    }
}
