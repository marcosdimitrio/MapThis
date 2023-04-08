using MapThis.CommonServices.UniqueVariableNames.Interfaces;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace MapThis.CommonServices.UniqueVariableNames
{
    [Export(typeof(IUniqueVariableNameGenerator))]
    public class UniqueVariableNameGenerator : IUniqueVariableNameGenerator
    {
        public string GetUniqueVariableName(string variableName, IList<IParameterSymbol> otherParametersInMethod)
        {
            var resultingVariableName = variableName;
            var counter = 2;

            do
            {
                if (!otherParametersInMethod.Any(x => x.Name == resultingVariableName))
                {
                    return resultingVariableName;
                };

                resultingVariableName = $"{variableName}{counter}";
                counter++;
            }
            while (true);
        }
    }
}
