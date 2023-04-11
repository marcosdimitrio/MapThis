using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MapThis.CommonServices.AccessModifierIdentifiers.Interfaces
{
    public interface IAccessModifierIdentifier
    {
        IList<SyntaxToken> GetNewMethodAccessModifiers(IList<SyntaxToken> originalModifiers);
    }
}
