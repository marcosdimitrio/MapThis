using MapThis.Services.CompoundGenerator.Interfaces;
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
        public IList<ICompoundMethodGenerator> ChildrenCompoundGenerator { get; }

        public MapInformationDto(IList<SyntaxToken> accessModifiers, string firstParameterName, IList<PropertyToMapDto> propertiesToMap, ITypeSymbol sourceType, ITypeSymbol targetType, IList<ICompoundMethodGenerator> childrenCompoundGenerator)
        {
            AccessModifiers = accessModifiers;
            FirstParameterName = firstParameterName;
            PropertiesToMap = propertiesToMap;
            SourceType = sourceType;
            TargetType = targetType;
            ChildrenCompoundGenerator = childrenCompoundGenerator;
        }
    }
}
