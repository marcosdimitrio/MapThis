using MapThis.CommonServices.IdentifierNames.Interfaces;
using MapThis.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.CommonServices.IdentifierNames
{
    [Export(typeof(IIdentifierNameService))]
    public class IdentifierNameService : IIdentifierNameService
    {
        public TypeSyntax GetTypeSyntaxConsideringNamespaces(ITypeSymbol typeSymbol, IList<string> existingNamespaces, SyntaxGenerator syntaxGenerator)
        {
            if (typeSymbol.IsSimpleTypeWithAlias())
            {
                return IdentifierName(typeSymbol.ToDisplayString());
            }

            if (existingNamespaces.Any(x => x == typeSymbol.ContainingNamespace.ToDisplayString()))
            {
                return (TypeSyntax)syntaxGenerator.TypeExpression(typeSymbol);
            }

            return IdentifierName(typeSymbol.Name);
        }

    }
}
