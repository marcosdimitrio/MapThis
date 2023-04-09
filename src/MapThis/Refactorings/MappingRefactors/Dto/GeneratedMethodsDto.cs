using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MapThis.Refactorings.MappingRefactors.Dto
{
    public class GeneratedMethodsDto
    {
        public IList<MethodDeclarationSyntax> Blocks { get; set; }
        public IList<string> Namespaces { get; set; }
    }
}
