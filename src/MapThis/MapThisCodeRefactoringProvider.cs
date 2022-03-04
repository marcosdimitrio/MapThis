using MapThis.Dto;
using MapThis.Helpers;
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

namespace MapThis
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(MapThisCodeRefactoringProvider)), Shared]
    internal class MapThisCodeRefactoringProvider : CodeRefactoringProvider
    {
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

            var blocks = GetBlocks(methodSyntax, mapInformation);

            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(methodSyntax, blocks);
            return context.Document.WithSyntaxRoot(newRoot);
        }

        private IList<MethodDeclarationSyntax> GetBlocks(MethodDeclarationSyntax methodSyntax, MapInformationDto mapInformation)
        {
            var blocks = new List<MethodDeclarationSyntax>();

            var firstStatement = GetMappedObjectStatement(mapInformation);

            var firstBlockSyntax =
                MethodDeclaration(
                    IdentifierName(mapInformation.TargetType.Name),
                    Identifier("Map")
                )
                .WithModifiers(
                    TokenList(mapInformation.AccessModifiers)
                )
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier("item"))
                                .WithType(IdentifierName(mapInformation.SourceType.Name))
                        )
                    )
                )
                .WithBody(
                    Block(
                        firstStatement,
                        EmptyStatement()
                            .WithSemicolonToken(
                                MissingToken(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(CarriageReturnLineFeed, CarriageReturnLineFeed, Whitespace(new string(' ', methodSyntax.GetLeadingTrivia().FullSpan.Length + 4))))
                            ),
                        ReturnStatement(IdentifierName("newItem"))
                    )
                );

            blocks.Add(firstBlockSyntax);

            foreach (var childMapCollectionInformation in mapInformation.ChildrenMapCollectionInformation)
            {
                var mapListStatement = GetMappedListBody(childMapCollectionInformation);

                var blockSyntax =
                    MethodDeclaration(
                        GenericName(
                            Identifier("IList")
                        )
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(childMapCollectionInformation.TargetType.GetElementType().Name))
                            )
                        ),
                        Identifier("Map")
                    )
                    .WithModifiers(
                        TokenList(childMapCollectionInformation.AccessModifiers)
                    )
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(Identifier("source"))
                                    .WithType(
                                        GenericName(
                                            Identifier("IList"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    IdentifierName(childMapCollectionInformation.SourceType.GetElementType().Name)
                                                )
                                            )
                                        )
                                    )
                            )
                        )
                    )
                    .WithBody(mapListStatement);

                blocks.Add(blockSyntax);

                blocks.AddRange(GetBlocks(blockSyntax, childMapCollectionInformation.ChildMapInformation));
            }

            foreach (var childMapInformation in mapInformation.ChildrenMapInformation)
            {
                blocks.AddRange(GetBlocks(null, childMapInformation));
                //var statement = GetMappedObjectStatement(childMapInformation);

                //var blockSyntax =
                //    MethodDeclaration(
                //        IdentifierName(childMapInformation.TargetType.Name),
                //        Identifier("Map")
                //    )
                //    .WithModifiers(
                //        TokenList(childMapInformation.AccessModifiers)
                //    )
                //    .WithParameterList(
                //        ParameterList(
                //            SingletonSeparatedList(
                //                Parameter(Identifier("item"))
                //                    .WithType(IdentifierName(childMapInformation.SourceType.Name))
                //            )
                //        )
                //    )
                //    .WithBody(
                //        Block(
                //            statement,
                //            EmptyStatement()
                //                .WithSemicolonToken(
                //                    MissingToken(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(CarriageReturnLineFeed, CarriageReturnLineFeed, Whitespace(new string(' ', methodSyntax.GetLeadingTrivia().FullSpan.Length + 4))))
                //                ),
                //            ReturnStatement(IdentifierName("newItem"))
                //        )
                //    );

                //blocks.Add(blockSyntax);
            }

            return blocks;
        }

        private static async Task<MapInformationDto> GetMapInformation(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken);
            //var generator = SyntaxGenerator.GetGenerator(context.Document);

            var accessModifiers = methodSyntax.Modifiers.ToList();

            var firstParameterSymbol = methodSymbol.Parameters[0];
            var targetType = methodSymbol.ReturnType;

            var sourceMembers = firstParameterSymbol.Type.GetPublicProperties();
            var targetMembers = targetType.GetPublicProperties();

            var mapInformation = GetMap(accessModifiers, firstParameterSymbol.Type, targetType, firstParameterSymbol.Name, sourceMembers, targetMembers);

            return mapInformation;
        }

        private static MapInformationDto GetMap(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, IList<IPropertySymbol> sourceMembers, IList<IPropertySymbol> targetMembers)
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
                    targetListType = (INamedTypeSymbol)targetNamedType.GetElementType();
                    sourceListType = (INamedTypeSymbol)sourceNamedType.GetElementType();

                    var sourceListMembers = sourceListType.GetPublicProperties();
                    var targetListMembers = targetListType.GetPublicProperties();

                    //TODO: Map lists
                    //TODO: Check if map already exists
                    //TODO: Get VS tab size or replace whitespace with formatting
                    //TODO: Add usings (using System.Collections.Generic)

                    var privateAccessModifiers = GetNonPublicAccessModifiers(accessModifiers);

                    var childMapCollection = GetMapsForCollection(privateAccessModifiers, sourceProperty.Type, targetProperty.Type, "source");
                    childrenMapCollectionInformation.AddRange(childMapCollection);

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

        private static IList<MapCollectionInformationDto> GetMapsForCollection(IList<SyntaxToken> accessModifiers, ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName)
        {
            var newExpression = GetCollectionMapExpression(sourceType, targetType, firstParameterName);

            var sourceListType = (INamedTypeSymbol)sourceType.GetElementType();
            var targetListType = (INamedTypeSymbol)targetType.GetElementType();

            var sourceListMembers = sourceListType.GetPublicProperties();
            var targetListMembers = targetListType.GetPublicProperties();

            var childMapInformation = GetMap(accessModifiers, sourceListType, targetListType, "item", sourceListMembers, targetListMembers);

            var mapCollectionInformationDto = new MapCollectionInformationDto(accessModifiers, firstParameterName, sourceType, targetType, newExpression, childMapInformation);

            return new List<MapCollectionInformationDto>() { mapCollectionInformationDto };
        }

        private static StatementSyntax GetCollectionMapExpression(ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName)
        {
            return
                LocalFunctionStatement(
                    GenericName(Identifier("IList"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                IdentifierName(targetType.Name)
                            )
                        )
                    ),
                    Identifier("Map")
                )
                .WithModifiers(
                    TokenList(Token(SyntaxKind.PrivateKeyword))
                )
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                Identifier(firstParameterName))
                            .WithType(
                                GenericName(Identifier("IList"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            IdentifierName(sourceType.Name)
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
                .WithBody(
                    Block(
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList())))
                            .WithVariables(
                                SingletonSeparatedList(
                                    VariableDeclarator(
                                        Identifier("destination"))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            ObjectCreationExpression(
                                                GenericName(
                                                    Identifier("List"))
                                                .WithTypeArgumentList(
                                                    TypeArgumentList(
                                                        SingletonSeparatedList<TypeSyntax>(
                                                            IdentifierName(targetType.Name)))))
                                            .WithArgumentList(
                                                ArgumentList())))))),
                        ForEachStatement(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    TriviaList())),
                            Identifier("item"),
                            IdentifierName("source"),
                            Block(
                                SingletonList<StatementSyntax>(
                                    ExpressionStatement(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("destination"),
                                                IdentifierName("Add")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        InvocationExpression(
                                                            IdentifierName("Map"))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList(
                                                                    Argument(
                                                                        IdentifierName("item")
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        ReturnStatement(
                            IdentifierName("destination"))))
                .NormalizeWhitespace();
        }

        private StatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var propertyToMap in mapInformationDto.PropertiesToMap)
            {
                AssignmentExpressionSyntax assignment = propertyToMap.NewExpression;

                //if (propertyToMap.Target.Type.IsSimpleType())
                //{
                //    assignment = GetDirectAssignment(propertyToMap.TargetName, mapInformationDto.FirstParameterName, propertyToMap.SourceName);
                //}
                //else
                //{
                //    assignment = GetAssignmentWithMap(propertyToMap.TargetName, mapInformationDto.FirstParameterName, propertyToMap.SourceName);
                //}

                syntaxNodeOrTokenList.Add(assignment);
                syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
            }

            var statement =
                LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName(
                            Identifier(
                                TriviaList(),
                                SyntaxKind.VarKeyword,
                                "var",
                                "var",
                                TriviaList()
                            )
                        )
                    )
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier("newItem"))
                            .WithInitializer(
                                EqualsValueClause(
                                    ObjectCreationExpression(
                                        IdentifierName(mapInformationDto.TargetType.Name))
                                    .WithArgumentList(
                                        ArgumentList())
                                    .WithInitializer(
                                        InitializerExpression(
                                            SyntaxKind.ObjectInitializerExpression,
                                            SeparatedList<ExpressionSyntax>(
                                                syntaxNodeOrTokenList
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                );

            return statement;
        }

        private BlockSyntax GetMappedListBody(MapCollectionInformationDto mapCollectionInformationDto)
        {
            var statement =
                Block(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.VarKeyword,
                                    "var",
                                    "var",
                                    TriviaList())))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier("destination"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            GenericName(
                                                Identifier("List"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(mapCollectionInformationDto.TargetType.GetElementType().Name)))))
                                        .WithArgumentList(
                                            ArgumentList()
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    ForEachStatement(
                        IdentifierName(
                            Identifier(
                                TriviaList(),
                                SyntaxKind.VarKeyword,
                                "var",
                                "var",
                                TriviaList())),
                        Identifier("item"),
                        IdentifierName("source"),
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("destination"),
                                            IdentifierName("Add")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    InvocationExpression(
                                                        IdentifierName("Map"))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                Argument(
                                                                    IdentifierName("item")
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    ReturnStatement(
                        IdentifierName("destination")
                    )
                );

            return statement;
        }

        private static AssignmentExpressionSyntax GetDirectAssignment(string leftIdentifierName, string parameterName, string rightIdentifierName)
        {
            return
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(leftIdentifierName),
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(parameterName),
                        IdentifierName(rightIdentifierName))
                )
                .WithLeadingTrivia(ElasticCarriageReturnLineFeed);
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
