using Microsoft.CodeAnalysis;

namespace MapThis.CommonServices.ExistingMethodsControl.Dto
{
    public class ExistingMethodDto
    {
        public INamedTypeSymbol SourceType { get; set; }
        public INamedTypeSymbol TargetType { get; set; }
    }
}
