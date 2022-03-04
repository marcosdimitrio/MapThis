using Microsoft.CodeAnalysis;

namespace MapThis.Dto
{
    public class ExistingMethodDto
    {
        public ITypeSymbol SourceType { get; set; }
        public ITypeSymbol TargetType { get; set; }
    }
}
