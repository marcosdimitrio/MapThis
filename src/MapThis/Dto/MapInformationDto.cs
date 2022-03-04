using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapInformationDto
    {
        public string FirstParameterName { get; private set; }
        public IList<PropertyToMapDto> PropertiesToMap { get; private set; }
        public ITypeSymbol SourceType { get; private set; }
        public ITypeSymbol TargetType { get; private set; }

        public MapInformationDto(string firstParameterName, IList<PropertyToMapDto> propertiesToMap, ITypeSymbol sourceType, ITypeSymbol targetType)
        {
            FirstParameterName = firstParameterName;
            PropertiesToMap = propertiesToMap;
            SourceType = sourceType;
            TargetType = targetType;
        }
    }
}
