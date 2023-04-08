using MapThis.CommonServices.ExistingMethodsControl.Dto;
using MapThis.CommonServices.ExistingMethodsControl.Factories.Interfaces;
using MapThis.CommonServices.ExistingMethodsControl.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Factories.Interfaces;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Interfaces;
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
        private readonly IMethodGeneratorFactory CompoundMethodGeneratorFactory;
        private readonly IExistingMethodControlServiceFactory ExistingMethodControlFactory;

        [ImportingConstructor]
        public MappingInformationService(IMethodGeneratorFactory compoundMethodGeneratorFactory, IExistingMethodControlServiceFactory existingMethodControlFactory)
        {
            CompoundMethodGeneratorFactory = compoundMethodGeneratorFactory;
            ExistingMethodControlFactory = existingMethodControlFactory;
        }

        public IMethodGenerator GetCompoundMethodsGenerator(OptionsDto optionsDto, MethodDeclarationSyntax originalMethodSyntax, IMethodSymbol originalMethodSymbol, SyntaxNode root, CompilationUnitSyntax compilationUnitSyntax, SemanticModel semanticModel, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
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

            var existingMethodsControlService = ExistingMethodControlFactory.Create(existingMethodsList);

            var otherParametersInMethod = originalMethodSymbol.Parameters.ToList();

            var currentMethodInformationDto = new MethodInformationDto(accessModifiers, sourceType, targetType, firstParameterSymbol.Name, otherParametersInMethod);

            if (targetType.IsCollection() && sourceType.IsCollection())
            {
                return GetMapForCollection(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespacesList);
            }

            if (targetType.IsEnum() && sourceType.IsEnum())
            {
                return GetMapForEnum(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespacesList);
            }

            return GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, currentMethodInformationDto, existingMethodsControlService, existingNamespacesList);
        }

        private IMethodGenerator GetMapForSimpleType(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var childrenMethodGenerators = new List<IMethodGenerator>();

            var propertiesToMap = new List<PropertyToMapDto>();

            var sourceMembers = currentMethodInformationDto.SourceType.GetPublicProperties();
            var targetMembers = currentMethodInformationDto.TargetType.GetPublicProperties();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInPropertySymbols(targetProperty, sourceMembers);

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
                else if (targetNamedType.IsClass() && sourceNamedType.IsClass())
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceNamedType, targetNamedType, "item", new List<IParameterSymbol>());
                        var childMethodGenerator = GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }
                else if (targetNamedType.IsEnum() && sourceNamedType.IsEnum())
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceNamedType, targetNamedType, "item", new List<IParameterSymbol>());
                        var childMethodGenerator = GetMapForEnum(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }
            }

            var mapInformation = new MapInformationDto(currentMethodInformationDto, propertiesToMap, childrenMethodGenerators, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapInformation, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

        private IMethodGenerator GetMapForCollection(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var sourceElementType = (INamedTypeSymbol)currentMethodInformationDto.SourceType.GetElementType();
            var targetElementType = (INamedTypeSymbol)currentMethodInformationDto.TargetType.GetElementType();

            IMethodGenerator childMethodGenerator = null;

            var privateAccessModifiers = GetNewMethodAccessModifiers(currentMethodInformationDto.AccessModifiers);

            if (!currentMethodInformationDto.SourceType.IsCollectionOfSimpleType())
            {
                if (existingMethodsControlService.TryAddMethod(sourceElementType, targetElementType))
                {
                    var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceElementType, targetElementType, "item", new List<IParameterSymbol>());
                    childMethodGenerator = GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                }
            }
            else if (targetElementType.IsEnum() && sourceElementType.IsEnum())
            {
                if (existingMethodsControlService.TryAddMethod(sourceElementType, targetElementType))
                {
                    var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceElementType, targetElementType, "item", new List<IParameterSymbol>());
                    childMethodGenerator = GetMapForEnum(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService, existingNamespaces);
                }
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(currentMethodInformationDto, childMethodGenerator, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapCollectionInformationDto, codeAnalisysDependenciesDto, existingNamespaces);

            return methodGenerator;
        }

        private IMethodGenerator GetMapForEnum(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto currentMethodInformationDto, IExistingMethodsControlService existingMethodsControlService, IList<string> existingNamespaces)
        {
            var sourceMembers = currentMethodInformationDto.SourceType.GetMembers().Where(x => x.Kind == SymbolKind.Field).ToList();
            var targetMembers = currentMethodInformationDto.TargetType.GetMembers().Where(x => x.Kind == SymbolKind.Field).ToList();

            var childrenMethodGenerators = new List<IMethodGenerator>();

            var enumItemsToMap = new List<EnumItemToMapDto>();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInSymbols(targetProperty, sourceMembers);

                var enumItemToMap = new EnumItemToMapDto(targetProperty, currentMethodInformationDto.FirstParameterName);

                enumItemsToMap.Add(enumItemToMap);
            }

            var mapInformation = new MapEnumInformationDto(currentMethodInformationDto, enumItemsToMap, childrenMethodGenerators, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapInformation, codeAnalisysDependenciesDto, existingNamespaces);

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

        private static IPropertySymbol FindCorrespondingPropertyInPropertySymbols(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        private static ISymbol FindCorrespondingPropertyInSymbols(ISymbol targetProperty, IList<ISymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        private static IList<SyntaxToken> GetNewMethodAccessModifiers(IList<SyntaxToken> originalModifiers)
        {
            var listToRemove = new List<SyntaxKind>();
            var listToAdd = new List<SyntaxToken>();

            if (originalModifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)))
            {
                listToAdd.Add(Token(SyntaxKind.PrivateKeyword));
                listToRemove.Add(SyntaxKind.PublicKeyword);
            }

            if (originalModifiers.Any(x => x.IsKind(SyntaxKind.VirtualKeyword)))
            {
                listToRemove.Add(SyntaxKind.VirtualKeyword);
            }

            var newList = listToAdd.ToList();
            newList.AddRange(originalModifiers.Where(x => !listToRemove.Contains(x.Kind())).ToList());

            return newList;
        }

    }
}
