using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Enums.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerators.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.EnumMethodGenerators
{
    [Export(typeof(IEnumMethodGenerator))]
    public class EnumMethodGenerator : IEnumMethodGenerator
    {
        public MethodDeclarationSyntax Generate(MapEnumInformationDto mapEnumInformationDto, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var returnVariableName = GetUniqueVariableName("newItem", mapEnumInformationDto.MethodInformation.OtherParametersInMethod);

            var mappedObjectStatement = GetMappedObjectStatement(mapEnumInformationDto, returnVariableName, codeAnalysisDependenciesDto, existingNamespaces);

            var returnStatement =
                ReturnStatement(IdentifierName(returnVariableName))
                    .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));

            var statements = new List<StatementSyntax>();

            statements.Add(mappedObjectStatement);
            statements.Add(returnStatement);

            var targetTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapEnumInformationDto.MethodInformation.TargetType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);
            var sourceTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapEnumInformationDto.MethodInformation.SourceType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

            var methodDeclaration =
                MethodDeclaration(
                    targetTypeSyntax,
                    Identifier("Map")
                )
                .WithModifiers(
                    TokenList(mapEnumInformationDto.MethodInformation.AccessModifiers)
                )
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier(mapEnumInformationDto.MethodInformation.FirstParameterName))
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

        private LocalDeclarationStatementSyntax GetMappedObjectStatement(MapEnumInformationDto mapEnumInformationDto, string returnVariableName, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var enumItemToMapDto in mapEnumInformationDto.EnumsItemsToMap)
            {
                syntaxNodeOrTokenList.Add(GetPropertyExpression(mapEnumInformationDto, enumItemToMapDto, codeAnalysisDependenciesDto, existingNamespaces));
                syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
            };

            var defaultWithThrowStatement = GetDefaultWithThrowStatement(mapEnumInformationDto, existingNamespaces, codeAnalysisDependenciesDto);
            syntaxNodeOrTokenList.AddRange(defaultWithThrowStatement);

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
                                    SwitchExpression(
                                        IdentifierName(mapEnumInformationDto.MethodInformation.FirstParameterName))
                                    .WithArms(
                                        SeparatedList<SwitchExpressionArmSyntax>(syntaxNodeOrTokenList)
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

        private SwitchExpressionArmSyntax GetPropertyExpression(MapEnumInformationDto mapEnumInformationDto, EnumItemToMapDto enumItemToMapDto, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var enumItemName = enumItemToMapDto.TargetProperty.Name;

            var sourceTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapEnumInformationDto.MethodInformation.SourceType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);
            var targetTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapEnumInformationDto.MethodInformation.TargetType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

            var switchExpressionArmSyntax = SwitchExpressionArm(
                ConstantPattern(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        sourceTypeSyntax,
                        IdentifierName(enumItemName))),
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    targetTypeSyntax,
                    IdentifierName(enumItemName)));

            return switchExpressionArmSyntax;
        }

        private SyntaxNodeOrToken[] GetDefaultWithThrowStatement(MapEnumInformationDto mapEnumInformationDto, IList<string> existingNamespaces, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto)
        {
            var targetTypeSyntax = GetTypeSyntaxConsideringNamespaces(mapEnumInformationDto.MethodInformation.TargetType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

            var list = new SyntaxNodeOrToken[]
            {
                SwitchExpressionArm(
                    DiscardPattern(),
                    ThrowExpression(
                        ObjectCreationExpression(
                            IdentifierName("InvalidEnumArgumentException"))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        InterpolatedStringExpression(
                                            Token(SyntaxKind.InterpolatedStringStartToken))
                                        .WithContents(
                                            List(
                                                new InterpolatedStringContentSyntax[]{
                                                    InterpolatedStringText()
                                                    .WithTextToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.InterpolatedStringTextToken,
                                                            "Can't map from item \\\"",
                                                            "Can't map from item \"",
                                                            TriviaList())),
                                                    Interpolation(
                                                        IdentifierName(mapEnumInformationDto.MethodInformation.FirstParameterName)),
                                                    InterpolatedStringText()
                                                    .WithTextToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.InterpolatedStringTextToken,
                                                            "\\\" of enum ",
                                                            "\" of enum ",
                                                            TriviaList())),
                                                    Interpolation(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    IdentifierName(mapEnumInformationDto.MethodInformation.FirstParameterName),
                                                                    IdentifierName("GetType"))),
                                                            IdentifierName("FullName"))),
                                                    InterpolatedStringText()
                                                    .WithTextToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.InterpolatedStringTextToken,
                                                            " to destination enum ",
                                                            " to destination enum ",
                                                            TriviaList())),
                                                    Interpolation(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            TypeOfExpression(targetTypeSyntax),
                                                            IdentifierName("FullName"))),
                                                    InterpolatedStringText()
                                                    .WithTextToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.InterpolatedStringTextToken,
                                                            ".",
                                                            ".",
                                                            TriviaList()))})))))))),
                Token(SyntaxKind.CommaToken)
            };

            return list;
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
    }
}
