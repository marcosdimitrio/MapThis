using MapThis.DependencyInjection;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MethodGenerators.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

//TODO: Add usings (using System.Collections.Generic)
//TODO: Add private methods after all public methods
//TODO: Fix formatting
namespace MapThis
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MapThisCodeRefactoringProvider)), Shared]
    internal class MapThisCodeRefactoringProvider : CodeRefactoringProvider
    {
        private readonly IMethodGeneratorService MethodGeneratorService;

        public MapThisCodeRefactoringProvider()
            : this(DI.GetMethodGeneratorService())
        {
        }

        public MapThisCodeRefactoringProvider(IMethodGeneratorService methodGeneratorService)
        {
            MethodGeneratorService = methodGeneratorService;
        }

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var node = root.FindNode(context.Span);
            //root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());

            var methodDeclaration = node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
            {
                return;
            }
            if (methodDeclaration.Parent?.Kind() == SyntaxKind.InterfaceDeclaration)
            {
                return;
            }
            if (methodDeclaration.ParameterList.Parameters.Count == 0)
            {
                return;
            }

            Register(context, methodDeclaration);
        }

        private void Register(CodeRefactoringContext context, MethodDeclarationSyntax methodDeclaration)
        {
            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Map this", c => ReplaceAsync(context, methodDeclaration, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Document> ReplaceAsync(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var mapInformation = await GetMapInformation(context, methodSyntax, cancellationToken).ConfigureAwait(false);

            var blocks = GetBlocks(mapInformation);

            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(methodSyntax, blocks);
            return context.Document.WithSyntaxRoot(newRoot);
        }

        private static async Task<MapInformationDto> GetMapInformation(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
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

            var mapInformation = GetMap(accessModifiers, firstParameterSymbol.Type, targetType, firstParameterSymbol.Name, sourceMembers, targetMembers, existingMethods);

            return mapInformation;
        }

        private static MapInformationDto GetMap(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IPropertySymbol> sourceMembers, IList<IPropertySymbol> targetMembers, IList<IMethodSymbol> existingMethods)
        {
            var childrenMapInformation = new List<MapInformationDto>();
            var childrenMapCollectionInformation = new List<MapCollectionInformationDto>();

            var propertiesToMap = new List<PropertyToMapDto>();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindPropertyInSource(targetProperty, sourceMembers);

                AssignmentExpressionSyntax newExpression = null;
                INamedTypeSymbol targetListType = null;
                INamedTypeSymbol sourceListType = null;

                if (targetProperty.Type.IsSimpleType())
                {
                    newExpression = GetNewDirectConversion(firstParameterName, targetProperty.Name);
                }
                else if (
                    targetProperty.Type is INamedTypeSymbol targetNamedType && targetNamedType.IsCollection() &&
                    sourceProperty?.Type is INamedTypeSymbol sourceNamedType && sourceNamedType.IsCollection()
                )
                {
                    var childMapCollectionAlreadyExists = existingMethods.Any(x =>
                        SymbolEqualityComparer.Default.Equals(x.ReturnType, targetNamedType) &&
                        SymbolEqualityComparer.Default.Equals(x.Parameters.FirstOrDefault()?.Type, sourceNamedType)
                    );

                    if (!childMapCollectionAlreadyExists)
                    {
                        targetListType = (INamedTypeSymbol)targetNamedType.GetElementType();
                        sourceListType = (INamedTypeSymbol)sourceNamedType.GetElementType();

                        var sourceListMembers = sourceListType.GetPublicProperties();
                        var targetListMembers = targetListType.GetPublicProperties();

                        var privateAccessModifiers = GetNonPublicAccessModifiers(accessModifiers);

                        var childMapCollection = GetMapsForCollection(privateAccessModifiers, sourceProperty.Type, targetProperty.Type, existingMethods);
                        childrenMapCollectionInformation.AddRange(childMapCollection);
                    }

                    newExpression = GetConversionWithMap(firstParameterName, targetProperty.Name);
                }
                else
                {
                    throw new NotSupportedException("Cannot determine target property type");
                }

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, newExpression);

                propertiesToMap.Add(propertyToMap);
            }

            var mapInformation = new MapInformationDto(accessModifiers, firstParameterName, propertiesToMap, sourceType, targetType, childrenMapInformation, childrenMapCollectionInformation);

            return mapInformation;
        }

        private static IList<MapCollectionInformationDto> GetMapsForCollection(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, IList<IMethodSymbol> existingMethods)
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

                childMapInformation = GetMap(accessModifiers, sourceListType, targetListType, "item", sourceListMembers, targetListMembers, existingMethods);
            }

            var mapCollectionInformationDto = new MapCollectionInformationDto(accessModifiers, sourceType, targetType, childMapInformation);

            return new List<MapCollectionInformationDto>() { mapCollectionInformationDto };
        }

        private IList<MethodDeclarationSyntax> GetBlocks(MapInformationDto mapInformation)
        {
            var blocks = new List<MethodDeclarationSyntax>();

            var firstBlockSyntax = MethodGeneratorService.Generate(mapInformation);

            blocks.Add(firstBlockSyntax);

            foreach (var childMapCollectionInformation in mapInformation.ChildrenMapCollectionInformation)
            {
                var blockSyntax = MethodGeneratorService.Generate(childMapCollectionInformation);

                blocks.Add(blockSyntax);

                if (childMapCollectionInformation.ChildMapInformation != null)
                {
                    blocks.AddRange(GetBlocks(childMapCollectionInformation.ChildMapInformation));
                }
            }

            foreach (var childMapInformation in mapInformation.ChildrenMapInformation)
            {
                blocks.AddRange(GetBlocks(childMapInformation));
            }

            return blocks;
        }

        private static IPropertySymbol FindPropertyInSource(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        private static AssignmentExpressionSyntax GetNewDirectConversion(string identifierName, string propertyName)
        {
            // This will return an expression like "Id = item.Id"
            return
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(propertyName),
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(identifierName),
                        IdentifierName(propertyName)))
                .WithLeadingTrivia(ElasticCarriageReturnLineFeed);
            //.NormalizeWhitespace();
        }

        private static AssignmentExpressionSyntax GetConversionWithMap(string identifierName, string propertyName)
        {
            // This will return an expression like "Children = Map(item.Children)"
            return
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(propertyName),
                    InvocationExpression(
                        IdentifierName("Map"))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(identifierName),
                                        IdentifierName(propertyName)
                                    )
                                )
                            )
                        )
                    )
                )
                .WithLeadingTrivia(ElasticCarriageReturnLineFeed);
            //.NormalizeWhitespace();
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
