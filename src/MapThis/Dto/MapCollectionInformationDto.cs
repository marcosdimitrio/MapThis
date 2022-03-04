using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapCollectionInformationDto
    {
        public IList<SyntaxToken> AccessModifiers { get; }
        public ITypeSymbol SourceType { get; }
        public ITypeSymbol TargetType { get; }
        public MapInformationDto ChildMapInformation { get; }

        public MapCollectionInformationDto(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, MapInformationDto childMapInformation)
        {
            AccessModifiers = accessModifiers;
            SourceType = sourceType;
            TargetType = targetType;
            ChildMapInformation = childMapInformation;
        }

    }
}
