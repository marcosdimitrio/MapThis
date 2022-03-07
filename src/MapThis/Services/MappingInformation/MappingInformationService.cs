using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.CompoundGenerator.Factories.Interfaces;
using MapThis.Services.CompoundGenerator.Interfaces;
using MapThis.Services.MappingInformation.Interfaces;
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
        private readonly ICompoundGeneratorFactory CompoundGeneratorFactory;

        [ImportingConstructor]
        public MappingInformationService(ICompoundGeneratorFactory compoundGeneratorFactory)
        {
            CompoundGeneratorFactory = compoundGeneratorFactory;
        }

        public async Task<ICompoundGenerator> GetCompoundGenerators(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
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
                return GetMapForCollection(accessModifiers, sourceType, targetType, firstParameterSymbol.Name, existingMethods);
            }

            return GetMapForSimpleType(accessModifiers, sourceType, targetType, firstParameterSymbol.Name, existingMethods);
        }

        private ICompoundGenerator GetMapForSimpleType(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IMethodSymbol> existingMethods)
        {
            var childrenMapCollectionInformation = new List<ICompoundGenerator>();

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

                        var childMapCollection = GetMapForCollection(privateAccessModifiers, sourceProperty.Type, targetProperty.Type, "source", existingMethods);
                        childrenMapCollectionInformation.Add(childMapCollection);
                    }
                }

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, firstParameterName);

                propertiesToMap.Add(propertyToMap);
            }

            var mapInformation = new MapInformationDto(accessModifiers, firstParameterName, propertiesToMap, sourceType, targetType, childrenMapCollectionInformation);

            var compoundGenerator = CompoundGeneratorFactory.Get(mapInformation);

            return compoundGenerator;
        }

        private ICompoundGenerator GetMapForCollection(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IMethodSymbol> existingMethods)
        {
            var sourceListType = (INamedTypeSymbol)sourceType.GetElementType();
            var targetListType = (INamedTypeSymbol)targetType.GetElementType();

            var childMapInformationAlreadyExists = existingMethods.Any(x =>
                SymbolEqualityComparer.Default.Equals(x.ReturnType, targetListType) &&
                SymbolEqualityComparer.Default.Equals(x.Parameters.FirstOrDefault()?.Type, sourceListType)
            );

            ICompoundGenerator childCompoundGenerator = null;

            if (!childMapInformationAlreadyExists)
            {
                var privateAccessModifiers = GetNewMethodAccessModifiers(accessModifiers);

                childCompoundGenerator = GetMapForSimpleType(privateAccessModifiers, sourceListType, targetListType, "item", existingMethods);
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(accessModifiers, firstParameterName, sourceType, targetType, childCompoundGenerator);

            var compoundGenerator = CompoundGeneratorFactory.Get(mapCollectionInformationDto);

            return compoundGenerator;
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
                listToRemove.Add(SyntaxKind.PublicKeyword);
                listToAdd.Add(Token(SyntaxKind.PrivateKeyword));
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
