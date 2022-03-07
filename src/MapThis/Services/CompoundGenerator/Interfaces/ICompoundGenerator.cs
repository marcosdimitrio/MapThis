using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Services.CompoundGenerator.Interfaces
{
    public interface ICompoundGenerator
    {
        IList<MethodDeclarationSyntax> Generate();
        IList<INamespaceSymbol> GetNamespaces();
    }
}
