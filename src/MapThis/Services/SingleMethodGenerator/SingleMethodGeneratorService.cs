using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.SingleMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.SingleMethodGenerator
{
    [Export(typeof(ISingleMethodGeneratorService))]
    public class SingleMethodGeneratorService : ISingleMethodGeneratorService
    {
        public MethodDeclarationSyntax Generate(MapInformationDto mapInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            var returnVariableName = GetUniqueVariableName("newItem", mapInformation.MethodInformation.OtherParametersInMethod);

            var mappedObjectStatement = GetMappedObjectStatement(mapInformation, returnVariableName, codeAnalisysDependenciesDto);

            var nullCheckStatement = GetNullCheckStatementForClass(mapInformation);

            var returnStatement =
                ReturnStatement(IdentifierName(returnVariableName))
                    .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));

            var statements = new List<StatementSyntax>();

            if (nullCheckStatement != null) statements.Add(nullCheckStatement);
            statements.Add(mappedObjectStatement);
            statements.Add(returnStatement);

            var methodDeclaration =
                MethodDeclaration(
                    (TypeSyntax)codeAnalisysDependenciesDto.SyntaxGenerator.TypeExpression(mapInformation.MethodInformation.TargetType),
                    Identifier("Map")
                )
                .WithModifiers(
                    TokenList(mapInformation.MethodInformation.AccessModifiers)
                )
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier(mapInformation.MethodInformation.FirstParameterName))
                                .WithType((TypeSyntax)codeAnalisysDependenciesDto.SyntaxGenerator.TypeExpression(mapInformation.MethodInformation.SourceType))
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

        public MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            var mapListStatement = GetMappedListBody(childMapCollectionInformation, codeAnalisysDependenciesDto);

            var sourceListTypeName = GetSourceTypeListNameAsInterface(childMapCollectionInformation.MethodInformation.SourceType);

            var methodDeclaration =
                MethodDeclaration(
                    GenericName(childMapCollectionInformation.MethodInformation.TargetType.Name)
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                (TypeSyntax)codeAnalisysDependenciesDto.SyntaxGenerator.TypeExpression(childMapCollectionInformation.MethodInformation.TargetType.GetElementType())
                            )
                        )
                    ),
                    Identifier("Map")
                )
                .WithModifiers(
                    TokenList(childMapCollectionInformation.MethodInformation.AccessModifiers)
                )
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier(childMapCollectionInformation.MethodInformation.FirstParameterName))
                            .WithType(
                                GenericName(sourceListTypeName)
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList<TypeSyntax>(
                                            (TypeSyntax)codeAnalisysDependenciesDto.SyntaxGenerator.TypeExpression(childMapCollectionInformation.MethodInformation.SourceType.GetElementType())
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

        private LocalDeclarationStatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto, string returnVariableName, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
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
                                Identifier(returnVariableName))
                            .WithInitializer(
                                EqualsValueClause(
                                    ObjectCreationExpression(
                                        (TypeSyntax)codeAnalisysDependenciesDto.SyntaxGenerator.TypeExpression(mapInformationDto.MethodInformation.TargetType))
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
                            EndOfLine(Environment.NewLine))));

            return localDeclarationStatement;
        }

        private BlockSyntax GetMappedListBody(MapCollectionInformationDto mapCollectionInformationDto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto)
        {
            var destinationVariableName = GetUniqueVariableName("destination", mapCollectionInformationDto.MethodInformation.OtherParametersInMethod);

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
                                    Identifier(destinationVariableName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                        GenericName(
                                            Identifier("List"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    (TypeSyntax)codeAnalisysDependenciesDto.SyntaxGenerator.TypeExpression(mapCollectionInformationDto.MethodInformation.TargetType.GetElementType())))
                                                )
                                        )
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
                                EndOfLine(Environment.NewLine))));

            var nullCheckStatement = GetNullCheckStatementForCollection(mapCollectionInformationDto);

            var forEachVariableName = GetUniqueVariableName("item", mapCollectionInformationDto.MethodInformation.OtherParametersInMethod);

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
                    IdentifierName(mapCollectionInformationDto.MethodInformation.FirstParameterName),
                    Block(
                        SingletonList<StatementSyntax>(
                            ExpressionStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(destinationVariableName),
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
                .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));

            var returnStatement =
                ReturnStatement(
                    IdentifierName(destinationVariableName)
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
                        IdentifierName(mapInformationDto.MethodInformation.FirstParameterName),
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
                .WithTrailingTrivia(TriviaList(EndOfLine(Environment.NewLine), EndOfLine(Environment.NewLine)));
        }

        private IfStatementSyntax GetNullCheckStatementForCollection(MapCollectionInformationDto mapCollectionInformationDto)
        {
            if (!mapCollectionInformationDto.Options.NullChecking) return null;

            return
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        IdentifierName(mapCollectionInformationDto.MethodInformation.FirstParameterName),
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
                .WithTrailingTrivia(TriviaList(EndOfLine(Environment.NewLine)))
                .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));
        }

        private SyntaxNodeOrToken GetPropertyExpression(PropertyToMapDto propertyToMap)
        {
            if (propertyToMap.Target.Type.IsSimpleType() || propertyToMap.Target.Type.IsNullableSimpleType())
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
                .WithLeadingTrivia(EndOfLine(Environment.NewLine));
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
                .WithLeadingTrivia(EndOfLine(Environment.NewLine));
        }

        private static string GetUniqueVariableName(string variableName, IList<IParameterSymbol> otherParametersInMethod)
        {
            var resultingVariableName = variableName;
            var counter = 2;

            do
            {
                if (!otherParametersInMethod.Any(x => x.Name == resultingVariableName))
                {
                    return resultingVariableName;
                };

                resultingVariableName = $"{variableName}{counter}";
                counter++;
            }
            while (true);
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
