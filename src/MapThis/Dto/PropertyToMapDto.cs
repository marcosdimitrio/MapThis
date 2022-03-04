using Microsoft.CodeAnalysis;

namespace MapThis.Dto
{
    public class PropertyToMapDto
    {
        public string SourceName => Source?.Name ?? Target.Name;
        public string TargetName => Target.Name;
        public SyntaxNode NewExpression { get; private set; }
        public bool IsTargetValueType => Target.Type.IsValueType;
        public IPropertySymbol Source { get; private set; }
        public IPropertySymbol Target { get; private set; } // TODO: This can be null, but is freely accessible

        public PropertyToMapDto(IPropertySymbol source, IPropertySymbol target, SyntaxNode newExpression)
        {
            Source = source;
            Target = target;
            NewExpression = newExpression;
        }
    }
}
