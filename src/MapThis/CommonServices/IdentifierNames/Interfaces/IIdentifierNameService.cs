using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;

namespace MapThis.CommonServices.IdentifierNames.Interfaces
{
    public interface IIdentifierNameService
    {
        TypeSyntax GetTypeSyntaxConsideringNamespaces(ITypeSymbol typeSymbol, IList<string> existingNamespaces, SyntaxGenerator syntaxGenerator);
    }
}
