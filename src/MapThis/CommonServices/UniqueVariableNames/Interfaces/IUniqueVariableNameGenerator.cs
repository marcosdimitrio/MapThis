using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.CommonServices.UniqueVariableNames.Interfaces
{
    public interface IUniqueVariableNameGenerator
    {
        string GetUniqueVariableName(string variableName, IList<IParameterSymbol> otherParametersInMethod);
    }
}
