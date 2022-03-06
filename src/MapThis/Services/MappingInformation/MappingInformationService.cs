using MapThis.Dto;
using MapThis.Helpers;
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

        public async Task<MapInformationDto> GetMapInformation(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken);
            //var generator = SyntaxGenerator.GetGenerator(context.Document);

            var accessModifiers = methodSyntax.Modifiers.ToList();

            var firstParameterSymbol = methodSymbol.Parameters[0];
            var targetType = methodSymbol.ReturnType;

            var sourceMembers = firstParameterSymbol.Type.GetPublicProperties();
            var targetMembers = targetType.GetPublicProperties();

            var existingMethods = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Select(x => semanticModel.GetDeclaredSymbol(x, cancellationToken))
                .ToList();

            var mapInformation = GetMapForSimpleType(accessModifiers, firstParameterSymbol.Type, targetType, firstParameterSymbol.Name, sourceMembers, targetMembers, existingMethods);

            return mapInformation;
        }

        private MapInformationDto GetMapForSimpleType(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IPropertySymbol> sourceMembers, IList<IPropertySymbol> targetMembers, IList<IMethodSymbol> existingMethods)
        {
            var childrenMapInformation = new List<MapInformationDto>();
            var childrenMapCollectionInformation = new List<MapCollectionInformationDto>();

            var propertiesToMap = new List<PropertyToMapDto>();

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
                        var targetListType = (INamedTypeSymbol)targetNamedType.GetElementType();
                        var sourceListType = (INamedTypeSymbol)sourceNamedType.GetElementType();

                        var sourceListMembers = sourceListType.GetPublicProperties();
                        var targetListMembers = targetListType.GetPublicProperties();

                        var privateAccessModifiers = GetNonPublicAccessModifiers(accessModifiers);

                        var childMapCollection = GetMapsForCollection(privateAccessModifiers, sourceProperty.Type, targetProperty.Type, existingMethods);
                        childrenMapCollectionInformation.AddRange(childMapCollection);
                    }
                }

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, firstParameterName);

                propertiesToMap.Add(propertyToMap);
            }

            var mapInformation = new MapInformationDto(accessModifiers, firstParameterName, propertiesToMap, sourceType, targetType, childrenMapInformation, childrenMapCollectionInformation);

            return mapInformation;
        }

        private IList<MapCollectionInformationDto> GetMapsForCollection(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, IList<IMethodSymbol> existingMethods)
        {
            var sourceListType = (INamedTypeSymbol)sourceType.GetElementType();
            var targetListType = (INamedTypeSymbol)targetType.GetElementType();

            var childMapInformationAlreadyExists = existingMethods.Any(x =>
                SymbolEqualityComparer.Default.Equals(x.ReturnType, targetListType) &&
                SymbolEqualityComparer.Default.Equals(x.Parameters.FirstOrDefault()?.Type, sourceListType)
            );

            MapInformationDto childMapInformation = null;

            if (!childMapInformationAlreadyExists)
            {
                var sourceListMembers = sourceListType.GetPublicProperties();
                var targetListMembers = targetListType.GetPublicProperties();

                childMapInformation = GetMapForSimpleType(accessModifiers, sourceListType, targetListType, "item", sourceListMembers, targetListMembers, existingMethods);
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(accessModifiers, sourceType, targetType, childMapInformation);

            return new List<MapCollectionInformationDto>() { mapCollectionInformationDto };
        }

        private static IPropertySymbol FindCorrespondingPropertyInSourceMembers(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        private static IList<SyntaxToken> GetNonPublicAccessModifiers(IList<SyntaxToken> originalModifiers)
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

            var newList = originalModifiers.Where(x => !listToRemove.Contains(x.Kind())).ToList();
            newList.AddRange(listToAdd);

            return newList;
        }

    }
}
