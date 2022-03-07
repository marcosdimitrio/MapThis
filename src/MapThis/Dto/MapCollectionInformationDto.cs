using MapThis.Services.CompoundGenerator.Interfaces;
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
        public ICompoundMethodGenerator ChildCompoundGenerator { get; }

        public MapCollectionInformationDto(IList<SyntaxToken> accessModifiers, string firstParameterName, ITypeSymbol sourceType, ITypeSymbol targetType, ICompoundMethodGenerator childCompoundGenerator)
        {
            AccessModifiers = accessModifiers;
            FirstParameterName = firstParameterName;
            SourceType = sourceType;
            TargetType = targetType;
            ChildCompoundGenerator = childCompoundGenerator;
        }

    }
}
