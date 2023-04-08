using MapThis.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MapThis.Services.MappingInformation.Interfaces
{
    public interface IMappingInformationService
    {
        IMethodGenerator GetCompoundMethodsGenerator(OptionsDto optionsDto, MethodDeclarationSyntax originalMethodSyntax, IMethodSymbol originalMethodSymbol, SyntaxNode root, CompilationUnitSyntax compilationUnitSyntax, SemanticModel semanticModel, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto);
    }
}
