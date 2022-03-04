using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapThis.Dto
{
    public class PropertyToMapDto
    {
        public string SourceName => Source?.Name ?? Target.Name;
        public string TargetName => Target.Name;
        public AssignmentExpressionSyntax NewExpression { get; }
        public bool IsTargetValueType => Target.Type.IsValueType;
        public IPropertySymbol Source { get; }
        public IPropertySymbol Target { get; } // TODO: This can be null, but is freely accessible

        public PropertyToMapDto(IPropertySymbol source, IPropertySymbol target, AssignmentExpressionSyntax newExpression)
        {
            Source = source;
            Target = target;
            NewExpression = newExpression;
        }
    }
}
