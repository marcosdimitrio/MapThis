using MapThis.Dto;
using MapThis.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapThis.Services.MappingInformation.Interfaces
{
    public interface IMappingInformationService
    {
        ICompoundMethodGenerator GetCompoundMethodsGenerator(OptionsDto optionsDto, MethodDeclarationSyntax originalMethodSyntax, IMethodSymbol originalMethodSymbol, SyntaxNode root, CompilationUnitSyntax compilationUnitSyntax, SemanticModel semanticModel, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto);
    }
}
