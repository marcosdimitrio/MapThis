using MapThis.CommonServices.ExistingMethodsControl.Dto;
using MapThis.CommonServices.ExistingMethodsControl.Factories.Interfaces;
using MapThis.Dto;
using MapThis.Services.MappingInformation.Interfaces;
using MapThis.Services.MappingInformation.MethodConstructors.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace MapThis.Services.MappingInformation
{
    [Export(typeof(IMappingInformationService))]
    public class MappingInformationService : IMappingInformationService
    {
        private readonly IExistingMethodsControlServiceFactory ExistingMethodsControlServiceFactory;
        private readonly IRecursiveMethodConstructor RecursiveMethodConstructor;

        [ImportingConstructor]
        public MappingInformationService(IExistingMethodsControlServiceFactory existingMethodsControlServiceFactory, IRecursiveMethodConstructor recursiveMethodConstructor)
        {
            ExistingMethodsControlServiceFactory = existingMethodsControlServiceFactory;
            RecursiveMethodConstructor = recursiveMethodConstructor;
        }

        public IMethodGenerator GetMethodGenerator(OptionsDto optionsDto, MethodDeclarationSyntax originalMethodSyntax, IMethodSymbol originalMethodSymbol, SyntaxNode root, CompilationUnitSyntax compilationUnitSyntax, SemanticModel semanticModel, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            var accessModifiers = originalMethodSyntax.Modifiers.ToList();

            var firstParameterSymbol = originalMethodSymbol.Parameters[0];
            var sourceType = firstParameterSymbol.Type;
            var targetType = originalMethodSymbol.ReturnType;

            var existingMethodsList = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(x => semanticModel.GetDeclaredSymbol(x))
                .Select(x => new ExistingMethodDto()
                {
                    SourceType = x.Parameters.FirstOrDefault()?.Type as INamedTypeSymbol,
                    TargetType = x.ReturnType as INamedTypeSymbol,
                })
                .ToList();

            var existingNamespacesList = GetExistingNamespacesList(compilationUnitSyntax, originalMethodSymbol);

            var existingMethodsControlService = ExistingMethodsControlServiceFactory.Create(existingMethodsList);

            var otherParametersInMethod = originalMethodSymbol.Parameters.ToList();

            var currentMethodInformationDto = new MethodInformationDto(accessModifiers, sourceType, targetType, firstParameterSymbol.Name, otherParametersInMethod);

            return RecursiveMethodConstructor.GetMap(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespacesList);
        }

        private static IList<string> GetExistingNamespacesList(CompilationUnitSyntax compilationUnitSyntax, IMethodSymbol originalMethodSymbol)
        {
            var existingNamespacesList = new List<string>();

            var usings = compilationUnitSyntax
                .Usings
                .Select(x => x.Name.ToFullString())
                .ToList();

            existingNamespacesList.AddRange(usings);

            if (originalMethodSymbol.ContainingNamespace != null)
            {
                existingNamespacesList.Add(originalMethodSymbol.ContainingNamespace.ToDisplayString());
            }

            var sourceNamespace = originalMethodSymbol.Parameters[0].Type.ContainingNamespace?.ToDisplayString();
            var targetNamespace = originalMethodSymbol.ReturnType.ContainingNamespace?.ToDisplayString();

            existingNamespacesList.Add(sourceNamespace);
            existingNamespacesList.Add(targetNamespace);

            return existingNamespacesList;
        }

    }
}
