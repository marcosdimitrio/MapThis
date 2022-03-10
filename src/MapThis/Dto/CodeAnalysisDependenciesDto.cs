using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace MapThis.Dto
{
    public class CodeAnalysisDependenciesDto
    {
        public SyntaxGenerator SyntaxGenerator { get; set; }
        public Compilation Compilation { get; set; }
    }
}
