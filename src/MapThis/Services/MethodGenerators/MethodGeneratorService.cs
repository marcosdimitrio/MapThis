using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MethodGenerators.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MethodGenerators
{
    [Export(typeof(IMethodGeneratorService))]
    public class MethodGeneratorService : IMethodGeneratorService
    {
        public MethodDeclarationSyntax Generate(MapInformationDto mapInformation)
        {
            var mappedObjectStatement = GetMappedObjectStatement(mapInformation);

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
                            Parameter(Identifier(mapInformation.FirstParameterName))
                                .WithType(IdentifierName(mapInformation.SourceType.Name))
                        )
                    )
                )
                .WithBody(
                    Block(
                        mappedObjectStatement,
                        ReturnStatement(IdentifierName("newItem"))
                    )
                );

            return firstBlockSyntax;
        }

        public MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation)
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
                                    GenericName(Identifier("IList"))
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

            return blockSyntax;
        }

        private StatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var propertyToMap in mapInformationDto.PropertiesToMap)
            {
                syntaxNodeOrTokenList.Add(GetPropertyExpression(propertyToMap));
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

        private SyntaxNodeOrToken GetPropertyExpression(PropertyToMapDto propertyToMap)
        {
            if (propertyToMap.Target.Type.IsSimpleType())
            {
                return GetNewDirectConversion(propertyToMap.ParameterName, propertyToMap.Target.Name);
            }

            return GetConversionWithMap(propertyToMap.ParameterName, propertyToMap.Target.Name);
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

    }
}
