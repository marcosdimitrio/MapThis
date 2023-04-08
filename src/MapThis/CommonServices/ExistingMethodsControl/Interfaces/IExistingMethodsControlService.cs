using Microsoft.CodeAnalysis;

namespace MapThis.CommonServices.ExistingMethodsControl.Interfaces
{
    public interface IExistingMethodsControlService
    {
        bool TryAddMethod(INamedTypeSymbol sourceType, INamedTypeSymbol targetType);
    }
}
