using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.ExistingMethodsControl.Dto;
using MapThis.Services.ExistingMethodsControl.Factories.Interfaces;
using MapThis.Services.ExistingMethodsControl.Interfaces;
using MapThis.Services.MappingInformation.Interfaces;
using MapThis.Services.MethodGenerator.Factories.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MappingInformation
{
    [Export(typeof(IMappingInformationService))]
    public class MappingInformationService : IMappingInformationService
    {
        private readonly ICompoundMethodGeneratorFactory CompoundMethodGeneratorFactory;
        private readonly IExistingMethodControlFactory ExistingMethodControlFactory;

        [ImportingConstructor]
        public MappingInformationService(ICompoundMethodGeneratorFactory compoundMethodGeneratorFactory, IExistingMethodControlFactory existingMethodControlFactory)
        {
            CompoundMethodGeneratorFactory = compoundMethodGeneratorFactory;
            ExistingMethodControlFactory = existingMethodControlFactory;
        }

        public ICompoundMethodGenerator GetCompoundMethodsGenerator(OptionsDto optionsDto, MethodDeclarationSyntax originalMethodSyntax, IMethodSymbol originalMethodSymbol, SyntaxNode root, CompilationUnitSyntax compilationUnitSyntax, SemanticModel semanticModel, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
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

            //var a = sourceType.ContainingNamespace

            var existingNamespacesList = GetExistingNamespacesList(compilationUnitSyntax, originalMethodSymbol);

            var existingMethodsControlService = ExistingMethodControlFactory.Create(existingMethodsList);

            var otherParametersInMethod = originalMethodSymbol.Parameters.ToList();

            var currentMethodInformationDto = new MethodInformationDto(accessModifiers, sourceType, targetType, firstParameterSymbol.Name, otherParametersInMethod);

            if (targetType.IsCollection() && sourceType.IsCollection())
            {
                return GetMapForCollection(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespacesList);
            }

            return GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespacesList);
        }

        private ICompoundMethodGenerator GetMapForSimpleType(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var childrenMethodGenerators = new List<ICompoundMethodGenerator>();

            var propertiesToMap = new List<PropertyToMapDto>();

            var sourceMembers = currentMethodInformationDto.SourceType.GetPublicProperties();
            var targetMembers = currentMethodInformationDto.TargetType.GetPublicProperties();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInSourceMembers(targetProperty, sourceMembers);

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, currentMethodInformationDto.FirstParameterName);

                propertiesToMap.Add(propertyToMap);

                var targetNamedType = targetProperty.Type as INamedTypeSymbol;
                var sourceNamedType = sourceProperty?.Type as INamedTypeSymbol;

                if (targetNamedType == null || sourceNamedType == null)
                {
                    continue;
                }

                var privateAccessModifiers = GetNewMethodAccessModifiers(currentMethodInformationDto.AccessModifiers);

                if (targetNamedType.IsCollection() && sourceNamedType.IsCollection())
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceProperty.Type, targetProperty.Type, "source", new List<IParameterSymbol>());
                        var childMethodGenerator = GetMapForCollection(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }

                if (targetNamedType.IsClass() && sourceNamedType.IsClass())
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceNamedType, targetNamedType, "item", new List<IParameterSymbol>());
                        var childMethodGenerator = GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }
            }

            var mapInformation = new MapInformationDto(currentMethodInformationDto, propertiesToMap, childrenMethodGenerators, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapInformation, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

        private ICompoundMethodGenerator GetMapForCollection(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var sourceElementType = (INamedTypeSymbol)currentMethodInformationDto.SourceType.GetElementType();
            var targetElementType = (INamedTypeSymbol)currentMethodInformationDto.TargetType.GetElementType();

            ICompoundMethodGenerator childMethodGenerator = null;

            if (!(currentMethodInformationDto.SourceType.IsCollectionOfSimpleType()))
            {
                if (existingMethodsControlService.TryAddMethod(sourceElementType, targetElementType))
                {
                    var privateAccessModifiers = GetNewMethodAccessModifiers(currentMethodInformationDto.AccessModifiers);
                    var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceElementType, targetElementType, "item", new List<IParameterSymbol>());
                    childMethodGenerator = GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                }
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(currentMethodInformationDto, childMethodGenerator, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapCollectionInformationDto, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
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

        private static IPropertySymbol FindCorrespondingPropertyInSourceMembers(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        private static IList<SyntaxToken> GetNewMethodAccessModifiers(IList<SyntaxToken> originalModifiers)
        {
            var listToRemove = new List<SyntaxKind>();
            var listToAdd = new List<SyntaxToken>();

            if (originalModifiers.Any(x => x.Kind() == SyntaxKind.PublicKeyword))
            {
                listToAdd.Add(Token(SyntaxKind.PrivateKeyword));
                listToRemove.Add(SyntaxKind.PublicKeyword);
            }

            if (originalModifiers.Any(x => x.Kind() == SyntaxKind.VirtualKeyword))
            {
                listToRemove.Add(SyntaxKind.VirtualKeyword);
            }

            var newList = listToAdd.ToList();
            newList.AddRange(originalModifiers.Where(x => !listToRemove.Contains(x.Kind())).ToList());

            return newList;
        }

    }
}
