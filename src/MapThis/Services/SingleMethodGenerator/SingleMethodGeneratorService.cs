using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.SingleMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.SingleMethodGenerator
{
    [Export(typeof(ISingleMethodGeneratorService))]
    public class SingleMethodGeneratorService : ISingleMethodGeneratorService
    {
        public MethodDeclarationSyntax Generate(MapInformationDto mapInformation)
        {
            var mappedObjectStatement = GetMappedObjectStatement(mapInformation);

            var nullCheckStatement = GetNullCheckStatementForClass(mapInformation);

            var returnStatement =
                ReturnStatement(IdentifierName("newItem"))
                    .WithLeadingTrivia(TriviaList(LineFeed));

            var statements = new List<StatementSyntax>();

            if (nullCheckStatement != null) statements.Add(nullCheckStatement);
            statements.Add(mappedObjectStatement);
            statements.Add(returnStatement);

            var methodDeclaration =
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
                        statements
                    )
                );

            return methodDeclaration;
        }

        public MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation)
        {
            var mapListStatement = GetMappedListBody(childMapCollectionInformation);

            var sourceListTypeName = GetSourceTypeListNameAsInterface(childMapCollectionInformation.SourceType);

            var methodDeclaration =
                MethodDeclaration(
                    GenericName(childMapCollectionInformation.TargetType.Name)
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
                            Parameter(Identifier(childMapCollectionInformation.FirstParameterName))
                            .WithType(
                                GenericName(sourceListTypeName)
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

            return methodDeclaration;
        }

        private LocalDeclarationStatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var propertyToMap in mapInformationDto.PropertiesToMap)
            {
                syntaxNodeOrTokenList.Add(GetPropertyExpression(propertyToMap));
                syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
            }

            var localDeclarationStatement =
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
                )
                .WithSemicolonToken(
                    Token(
                        TriviaList(),
                        SyntaxKind.SemicolonToken,
                        TriviaList(
                            LineFeed)));

            return localDeclarationStatement;
        }

        private BlockSyntax GetMappedListBody(MapCollectionInformationDto mapCollectionInformationDto)
        {
            var returnVariableName = "destination";

            var variableDeclaration =
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
                                    Identifier(returnVariableName))
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
                    )
                    .WithSemicolonToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.SemicolonToken,
                            TriviaList(
                                LineFeed)));

            var nullCheckStatement = GetNullCheckStatementForCollection(mapCollectionInformationDto);

            var forEachVariableName = GetForEachVariableName(mapCollectionInformationDto);

            var forEachStatement =
                ForEachStatement(
                    IdentifierName(
                        Identifier(
                            TriviaList(),
                            SyntaxKind.VarKeyword,
                            "var",
                            "var",
                            TriviaList())),
                    Identifier(forEachVariableName),
                    IdentifierName(mapCollectionInformationDto.FirstParameterName),
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
                                                                IdentifierName(forEachVariableName)
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
                )
                .WithLeadingTrivia(TriviaList(LineFeed));

            var returnStatement =
                ReturnStatement(
                    IdentifierName("destination")
                );

            var statements = new List<StatementSyntax>();

            statements.Add(variableDeclaration);
            if (nullCheckStatement != null) statements.Add(nullCheckStatement);
            statements.Add(forEachStatement);
            statements.Add(returnStatement);

            var blockSyntax =
                Block(
                    statements
                );

            return blockSyntax;
        }

        private IfStatementSyntax GetNullCheckStatementForClass(MapInformationDto mapInformationDto)
        {
            if (!mapInformationDto.Options.NullChecking) return null;

            return
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        IdentifierName(mapInformationDto.FirstParameterName),
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression)),
                    ReturnStatement(
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression))
                    .WithReturnKeyword(
                        Token(
                            TriviaList(),
                            SyntaxKind.ReturnKeyword,
                            TriviaList(
                                Space)))
                )
                .WithCloseParenToken(
                    Token(
                        TriviaList(),
                        SyntaxKind.CloseParenToken,
                        TriviaList(
                            Space)))
                .WithTrailingTrivia(TriviaList(LineFeed, LineFeed));
        }

        private IfStatementSyntax GetNullCheckStatementForCollection(MapCollectionInformationDto mapCollectionInformationDto)
        {
            if (!mapCollectionInformationDto.Options.NullChecking) return null;

            return
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        IdentifierName(mapCollectionInformationDto.FirstParameterName),
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression)),
                    ReturnStatement(
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression))
                    .WithReturnKeyword(
                        Token(
                            TriviaList(),
                            SyntaxKind.ReturnKeyword,
                            TriviaList(
                                Space)))
                )
                .WithCloseParenToken(
                    Token(
                        TriviaList(),
                        SyntaxKind.CloseParenToken,
                        TriviaList(
                            Space)))
                .WithTrailingTrivia(TriviaList(LineFeed))
                .WithLeadingTrivia(TriviaList(LineFeed));
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

        private static string GetForEachVariableName(MapCollectionInformationDto mapCollectionInformationDto)
        {
            if ("item" != mapCollectionInformationDto.FirstParameterName)
            {
                return "item";
            }

            var className = mapCollectionInformationDto.TargetType.GetElementType().Name;

            var classNameCamelCase = char.ToLowerInvariant(className[0]) + className.Substring(1);

            return $"{classNameCamelCase}Item";
        }

        private static string GetSourceTypeListNameAsInterface(ITypeSymbol sourceType)
        {
            var name = sourceType.Name;

            if (name == "List" || name == "Collection")
            {
                return $"I{name}";
            }

            return name;
        }

    }
}
