using Microsoft.CodeAnalysis;

namespace MapThis.Dto
{
    public class PropertyToMapDto
    {
        public IPropertySymbol Source { get; } // This can be null
        public IPropertySymbol Target { get; }
        public string ParameterName { get; set; }

        public PropertyToMapDto(IPropertySymbol source, IPropertySymbol target, string parameterName)
        {
            Source = source;
            Target = target;
            ParameterName = parameterName;
        }
    }
}
