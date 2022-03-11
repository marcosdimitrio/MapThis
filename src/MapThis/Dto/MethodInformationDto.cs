using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.Dto
{
    public class MethodInformationDto
    {
        public IList<SyntaxToken> AccessModifiers { get; }
        public ITypeSymbol SourceType { get; }
        public ITypeSymbol TargetType { get; }
        public string FirstParameterName { get; }
        public IList<IParameterSymbol> OtherParametersInMethod { get; }

        public MethodInformationDto(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IParameterSymbol> otherParametersInMethod)
        {
            AccessModifiers = accessModifiers;
            SourceType = sourceType;
            TargetType = targetType;
            FirstParameterName = firstParameterName;
            OtherParametersInMethod = otherParametersInMethod;
        }

    }
}
