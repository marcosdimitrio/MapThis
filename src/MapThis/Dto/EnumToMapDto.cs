using Microsoft.CodeAnalysis;

namespace MapThis.Dto
{
    public class EnumItemToMapDto
    {
        public ISymbol TargetProperty { get; }
        public string FirstParameterName { get; }

        public EnumItemToMapDto(ISymbol targetProperty, string firstParameterName)
        {
            TargetProperty = targetProperty;
            FirstParameterName = firstParameterName;
        }
    }
}
