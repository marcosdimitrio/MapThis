using Microsoft.CodeAnalysis;

namespace MapThis.Dto
{
    public class PropertyToMapDto
    {
        private readonly IPropertySymbol Source;
        private readonly IPropertySymbol Target;

        public string SourceName => Source?.Name ?? Target.Name;
        public string TargetName => Target.Name;
        public SyntaxNode NewExpression { get; private set; }
        public bool IsTargetValueType => Target.Type.IsValueType;

        public PropertyToMapDto(IPropertySymbol source, IPropertySymbol target, SyntaxNode newExpression)
        {
            Source = source;
            Target = target;
            NewExpression = newExpression;
        }
    }
}
