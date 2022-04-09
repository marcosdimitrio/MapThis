using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.SingleMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
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
        public MethodDeclarationSyntax Generate(MapInformationDto mapInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            var returnVariableName = GetUniqueVariableName("newItem", mapInformation.MethodInformation.OtherParametersInMethod);

            var mappedObjectStatement = GetMappedObjectStatement(mapInformation, returnVariableName, codeAnalisysDependenciesDto, existingNamespaces);

            var nullCheckStatement = GetNullCheckStatementForClass(mapInformation);

            var returnStatement =
                ReturnStatement(IdentifierName(returnVariableName))
                    .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));

            var statements = new List<StatementSyntax>();

            if (nullCheckStatement != null) statements.Add(nullCheckStatement);
            statements.Add(mappedObjectStatement);
            statements.Add(returnStatement);

            var targetTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapInformation.MethodInformation.TargetType, existingNamespaces, codeAnalisysDependenciesDto.SyntaxGenerator);
            var sourceTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapInformation.MethodInformation.SourceType, existingNamespaces, codeAnalisysDependenciesDto.SyntaxGenerator);

            var methodDeclaration =
                MethodDeclaration(
                    targetTypeSyntax,
                    Identifier("Map")
                )
                .WithModifiers(
                    TokenList(mapInformation.MethodInformation.AccessModifiers)
                )
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier(mapInformation.MethodInformation.FirstParameterName))
                                .WithType(sourceTypeSyntax)
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

        public MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            var mapListStatement = GetMappedListBody(childMapCollectionInformation, codeAnalisysDependenciesDto, existingNamespaces);

            var sourceListTypeName = GetSourceTypeListNameAsInterface(childMapCollectionInformation.MethodInformation.SourceType);

            var targetTypeSyntax = GetTypeSyntaxConsideringNamespaces(childMapCollectionInformation.MethodInformation.TargetType.GetElementType(), existingNamespaces, codeAnalisysDependenciesDto.SyntaxGenerator);

            var sourceListTypeSyntax = GetTypeSyntaxConsideringNamespaces(childMapCollectionInformation.MethodInformation.SourceType.GetElementType(), existingNamespaces, codeAnalisysDependenciesDto.SyntaxGenerator);

            var methodDeclaration =
                MethodDeclaration(
                    GenericName(childMapCollectionInformation.MethodInformation.TargetType.Name)
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                targetTypeSyntax
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
                                            sourceListTypeSyntax
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

        private TypeSyntax GetTypeSyntaxConsideringNamespaces(ITypeSymbol typeSymbol, IList<string> existingNamespaces, SyntaxGenerator syntaxGenerator)
        {
            if (typeSymbol.IsSimpleTypeWithAlias())
            {
                return IdentifierName(typeSymbol.ToDisplayString());
            }

            if (existingNamespaces.Any(x => x == typeSymbol.ContainingNamespace.ToDisplayString()))
            {
                return (TypeSyntax)syntaxGenerator.TypeExpression(typeSymbol);
            }

            return IdentifierName(typeSymbol.Name);
        }

        private LocalDeclarationStatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto, string returnVariableName, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var propertyToMap in mapInformationDto.PropertiesToMap)
            {
                syntaxNodeOrTokenList.Add(GetPropertyExpression(propertyToMap));
                syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
            }

            var targetTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapInformationDto.MethodInformation.TargetType, existingNamespaces, codeAnalisysDependenciesDto.SyntaxGenerator);

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
                                    ObjectCreationExpression(targetTypeSyntax)
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

        private BlockSyntax GetMappedListBody(MapCollectionInformationDto mapCollectionInformationDto, CodeAnalysisDependenciesDto codeAnalisysDependenciesDto, IList<string> existingNamespaces)
        {
            var destinationVariableName = GetUniqueVariableName("destination", mapCollectionInformationDto.MethodInformation.OtherParametersInMethod);

            var targetListTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapCollectionInformationDto.MethodInformation.TargetType.GetElementType(), existingNamespaces, codeAnalisysDependenciesDto.SyntaxGenerator);

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
                                                    targetListTypeSyntax)
                                                )
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

            var forEachItemMapExpression = GetForEachItemMapExpression(mapCollectionInformationDto, forEachVariableName);

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
                                            forEachItemMapExpression
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
                .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));

            var returnStatement = GetReturnStatement(mapCollectionInformationDto, destinationVariableName);

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

        private ArgumentSyntax GetForEachItemMapExpression(MapCollectionInformationDto mapCollectionInformationDto, string forEachVariableName)
        {
            if (mapCollectionInformationDto.MethodInformation.SourceType.IsCollectionOfSimpleType())
            {
                return
                    Argument(
                        IdentifierName(forEachVariableName));
            }

            return
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
                );

        }

        private ReturnStatementSyntax GetReturnStatement(MapCollectionInformationDto mapCollectionInformationDto, string destinationVariableName)
        {
            if (mapCollectionInformationDto.MethodInformation.TargetType.IsArray())
            {
                return
                    ReturnStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(destinationVariableName),
                                IdentifierName("ToArray"))));
            }

            return
                ReturnStatement(
                    IdentifierName(destinationVariableName)
                );
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
            if (propertyToMap.Target.Type.IsSimpleType() || propertyToMap.Target.Type.IsNullableSimpleType() ||
                AreArraysOfTheSameSimpleType(propertyToMap.Target.Type, propertyToMap.Source.Type))
            {
                return GetNewDirectConversion(propertyToMap.ParameterName, propertyToMap.Target.Name);
            }

            return GetConversionWithMap(propertyToMap.ParameterName, propertyToMap.Target.Name);
        }

        private bool AreArraysOfTheSameSimpleType(ITypeSymbol target, ITypeSymbol source)
        {
            return target.IsArray() && source.IsArray() && 
                SymbolEqualityComparer.Default.Equals(target.GetElementType(), source.GetElementType());
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
