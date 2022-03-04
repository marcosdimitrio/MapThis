using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapCollectionInformationDto
    {
        public IList<SyntaxToken> AccessModifiers { get; }
        public string FirstParameterName { get; }
        public ITypeSymbol SourceType { get; }
        public ITypeSymbol TargetType { get; }
        public SyntaxNode NewExpression { get; }
        public MapInformationDto ChildMapInformation { get; }

        public MapCollectionInformationDto(IList<SyntaxToken> accessModifiers, string firstParameterName, ITypeSymbol sourceType, ITypeSymbol targetType, SyntaxNode newExpression, MapInformationDto childMapInformation)
        {
            AccessModifiers = accessModifiers;
            FirstParameterName = firstParameterName;
            SourceType = sourceType;
            TargetType = targetType;
            NewExpression = newExpression;
            ChildMapInformation = childMapInformation;
        }

    }
}
