using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Refactorings.MappingGenerator.Dto
{
    public class GeneratedMethodsDto
    {
        public IList<MethodDeclarationSyntax> Blocks { get; set; }
        public IList<INamespaceSymbol> Namespaces { get; set; }
    }
}
