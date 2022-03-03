using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MapInformationDto
    {
        public IParameterSymbol FirstParameter { get; private set; }
        public IList<PropertyToMapDto> PropertiesToMap { get; private set; }
        public ITypeSymbol ReturnType { get; private set; }

        public MapInformationDto(IParameterSymbol firstParameter, IList<PropertyToMapDto> propertiesToMap, ITypeSymbol returnType)
        {
            FirstParameter = firstParameter;
            PropertiesToMap = propertiesToMap;
            ReturnType = returnType;
        }
    }
}
