using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapInformationDto
    {
        public IList<SyntaxToken> AccessModifiers { get; }
        public string FirstParameterName { get; }
        public IList<PropertyToMapDto> PropertiesToMap { get; }
        public ITypeSymbol SourceType { get; }
        public ITypeSymbol TargetType { get; }
        public IList<MapCollectionInformationDto> ChildrenMapCollectionInformation { get; }

        public MapInformationDto(IList<SyntaxToken> accessModifiers, string firstParameterName, IList<PropertyToMapDto> propertiesToMap, ITypeSymbol sourceType, ITypeSymbol targetType, IList<MapCollectionInformationDto> childrenMapCollectionInformation)
        {
            AccessModifiers = accessModifiers;
            FirstParameterName = firstParameterName;
            PropertiesToMap = propertiesToMap;
            SourceType = sourceType;
            TargetType = targetType;
            ChildrenMapCollectionInformation = childrenMapCollectionInformation;
        }
    }
}
