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

        public ICompoundMethodGenerator GetCompoundMethodsGenerator(OptionsDto optionsDto, MethodDeclarationSyntax originalMethodSyntax, IMethodSymbol originalMethodSymbol, SyntaxNode root, SemanticModel semanticModel, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
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
                    SourceType = x.Parameters.FirstOrDefault()?.Type as INamedTypeSymbol, //TODO: See if it can be changed to First()
                    TargetType = x.ReturnType as INamedTypeSymbol,
                })
                .ToList();

            var existingMethodsControlService = ExistingMethodControlFactory.Create(existingMethodsList);

            var otherParametersInMethod = originalMethodSymbol.Parameters.ToList();

            var methodInformationDto = new MethodInformationDto(accessModifiers, sourceType, targetType, firstParameterSymbol.Name, otherParametersInMethod);

            if (targetType.IsCollection() && sourceType.IsCollection())
            {
                return GetMapForCollection(codeAnalisysDependenciesDto, optionsDto, methodInformationDto, existingMethodsControlService);
            }

            return GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, methodInformationDto, existingMethodsControlService);
        }

        private ICompoundMethodGenerator GetMapForSimpleType(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto methodInformationDto, IExistingMethodsControlService existingMethodsControlService)
        {
            var childrenMethodGenerators = new List<ICompoundMethodGenerator>();

            var propertiesToMap = new List<PropertyToMapDto>();

            var sourceMembers = methodInformationDto.SourceType.GetPublicProperties();
            var targetMembers = methodInformationDto.TargetType.GetPublicProperties();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInSourceMembers(targetProperty, sourceMembers);

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, methodInformationDto.FirstParameterName);

                propertiesToMap.Add(propertyToMap);

                var targetNamedType = targetProperty.Type as INamedTypeSymbol;
                var sourceNamedType = sourceProperty?.Type as INamedTypeSymbol;

                if (targetNamedType == null || sourceNamedType == null)
                {
                    continue;
                }

                var privateAccessModifiers = GetNewMethodAccessModifiers(methodInformationDto.AccessModifiers);

                if (targetNamedType.IsCollection() && sourceNamedType.IsCollection())
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceProperty.Type, targetProperty.Type, "source", new List<IParameterSymbol>());
                        var childMethodGenerator = GetMapForCollection(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }

                if (targetNamedType.IsClass() && sourceNamedType.IsClass())
                {
                    if (existingMethodsControlService.TryAddMethod(sourceNamedType, targetNamedType))
                    {
                        var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceNamedType, targetNamedType, "item", new List<IParameterSymbol>());
                        var childMethodGenerator = GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }
            }

            var mapInformation = new MapInformationDto(methodInformationDto, propertiesToMap, childrenMethodGenerators, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapInformation, codeAnalisysDependenciesDto);

            return methodGenerator;
        }

        private ICompoundMethodGenerator GetMapForCollection(CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, OptionsDto optionsDto, MethodInformationDto methodInformationDto, IExistingMethodsControlService existingMethodsControlService)
        {
            var sourceListType = (INamedTypeSymbol)methodInformationDto.SourceType.GetElementType();
            var targetListType = (INamedTypeSymbol)methodInformationDto.TargetType.GetElementType();

            ICompoundMethodGenerator childMethodGenerator = null;

            if (existingMethodsControlService.TryAddMethod(sourceListType, targetListType))
            {
                var privateAccessModifiers = GetNewMethodAccessModifiers(methodInformationDto.AccessModifiers);
                var childMethodInformationDto = new MethodInformationDto(privateAccessModifiers, sourceListType, targetListType, "item", new List<IParameterSymbol>());
                childMethodGenerator = GetMapForSimpleType(codeAnalisysDependenciesDto, optionsDto, childMethodInformationDto, existingMethodsControlService);
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(methodInformationDto, childMethodGenerator, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapCollectionInformationDto, codeAnalisysDependenciesDto);

            return methodGenerator;
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
