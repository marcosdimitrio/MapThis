using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.MappingInformation.Interfaces;
using MapThis.Services.MethodGenerator.Factories.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MappingInformation
{
    [Export(typeof(IMappingInformationService))]
    public class MappingInformationService : IMappingInformationService
    {
        private readonly ICompoundMethodGeneratorFactory CompoundMethodGeneratorFactory;

        [ImportingConstructor]
        public MappingInformationService(ICompoundMethodGeneratorFactory compoundMethodGeneratorFactory)
        {
            CompoundMethodGeneratorFactory = compoundMethodGeneratorFactory;
        }

        public async Task<ICompoundMethodGenerator> GetCompoundMethodsGenerator(OptionsDto optionsDto, CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken);
            //var generator = SyntaxGenerator.GetGenerator(context.Document);

            var accessModifiers = methodSyntax.Modifiers.ToList();

            var firstParameterSymbol = methodSymbol.Parameters[0];
            var sourceType = firstParameterSymbol.Type;
            var targetType = methodSymbol.ReturnType;

            var existingMethods = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(x => semanticModel.GetDeclaredSymbol(x, cancellationToken))
                .ToList();

            if (targetType.IsCollection() && sourceType.IsCollection())
            {
                return GetMapForCollection(optionsDto, accessModifiers, sourceType, targetType, firstParameterSymbol.Name, existingMethods);
            }

            return GetMapForSimpleType(optionsDto, accessModifiers, sourceType, targetType, firstParameterSymbol.Name, existingMethods);
        }

        private ICompoundMethodGenerator GetMapForSimpleType(OptionsDto optionsDto, IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IMethodSymbol> existingMethods)
        {
            var childrenMethodGenerators = new List<ICompoundMethodGenerator>();

            var propertiesToMap = new List<PropertyToMapDto>();

            var sourceMembers = sourceType.GetPublicProperties();
            var targetMembers = targetType.GetPublicProperties();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindCorrespondingPropertyInSourceMembers(targetProperty, sourceMembers);

                if (targetProperty.Type is INamedTypeSymbol targetNamedType && targetNamedType.IsCollection() &&
                    sourceProperty?.Type is INamedTypeSymbol sourceNamedType && sourceNamedType.IsCollection())
                {
                    var childMapCollectionAlreadyExists = existingMethods.Any(x =>
                        SymbolEqualityComparer.Default.Equals(x.ReturnType, targetNamedType) &&
                        SymbolEqualityComparer.Default.Equals(x.Parameters.FirstOrDefault()?.Type, sourceNamedType)
                    );

                    if (!childMapCollectionAlreadyExists)
                    {
                        var targetListType = targetNamedType.GetElementType();
                        var sourceListType = sourceNamedType.GetElementType();

                        var sourceListMembers = sourceListType.GetPublicProperties();
                        var targetListMembers = targetListType.GetPublicProperties();

                        var privateAccessModifiers = GetNewMethodAccessModifiers(accessModifiers);

                        var childMethodGenerator = GetMapForCollection(optionsDto, privateAccessModifiers, sourceProperty.Type, targetProperty.Type, "source", existingMethods);
                        childrenMethodGenerators.Add(childMethodGenerator);
                    }
                }

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, firstParameterName);

                propertiesToMap.Add(propertyToMap);
            }

            var mapInformation = new MapInformationDto(accessModifiers, firstParameterName, propertiesToMap, sourceType, targetType, childrenMethodGenerators, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapInformation);

            return methodGenerator;
        }

        private ICompoundMethodGenerator GetMapForCollection(OptionsDto optionsDto, IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IMethodSymbol> existingMethods)
        {
            var sourceListType = (INamedTypeSymbol)sourceType.GetElementType();
            var targetListType = (INamedTypeSymbol)targetType.GetElementType();

            var childMapInformationAlreadyExists = existingMethods.Any(x =>
                SymbolEqualityComparer.Default.Equals(x.ReturnType, targetListType) &&
                SymbolEqualityComparer.Default.Equals(x.Parameters.FirstOrDefault()?.Type, sourceListType)
            );

            ICompoundMethodGenerator childMethodGenerator = null;

            if (!childMapInformationAlreadyExists)
            {
                var privateAccessModifiers = GetNewMethodAccessModifiers(accessModifiers);

                childMethodGenerator = GetMapForSimpleType(optionsDto, privateAccessModifiers, sourceListType, targetListType, "item", existingMethods);
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(accessModifiers, firstParameterName, sourceType, targetType, childMethodGenerator, optionsDto);

            var methodGenerator = CompoundMethodGeneratorFactory.Get(mapCollectionInformationDto);

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
