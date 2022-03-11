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
        public ICompoundMethodGenerator ChildMethodGenerator { get; }
        public OptionsDto Options { get; }
        public IList<IParameterSymbol> OtherParametersInMethod { get; }

        public MapCollectionInformationDto(IList<SyntaxToken> accessModifiers, string firstParameterName, ITypeSymbol sourceType, ITypeSymbol targetType, ICompoundMethodGenerator childMethodGenerator, OptionsDto options, IList<IParameterSymbol> otherParametersInMethod)
        {
            AccessModifiers = accessModifiers;
            FirstParameterName = firstParameterName;
            SourceType = sourceType;
            TargetType = targetType;
            ChildMethodGenerator = childMethodGenerator;
            Options = options;
            OtherParametersInMethod = otherParametersInMethod;
        }

    }
}
