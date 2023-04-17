using MapThis.CommonServices.IdentifierNames.Interfaces;
using MapThis.CommonServices.UniqueVariableNames.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.SimpleTypes.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.SingleMethodGenerator
{
    [Export(typeof(ISingleMethodGeneratorService))]
    public class SingleMethodGeneratorService : ISingleMethodGeneratorService
    {
        private readonly IIdentifierNameService IdentifierNameService;
        private readonly IUniqueVariableNameGenerator UniqueVariableNameGenerator;

        [ImportingConstructor]
        public SingleMethodGeneratorService(IIdentifierNameService identifierNameService, IUniqueVariableNameGenerator uniqueVariableNameGenerator)
        {
            IdentifierNameService = identifierNameService;
            UniqueVariableNameGenerator = uniqueVariableNameGenerator;
        }

        public MethodDeclarationSyntax Generate(MapInformationDto mapInformation, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var returnVariableName = UniqueVariableNameGenerator.GetUniqueVariableName("newItem", mapInformation.MethodInformation.OtherParametersInMethod);

            var mappedObjectStatement = GetMappedObjectStatement(mapInformation, returnVariableName, codeAnalysisDependenciesDto, existingNamespaces);

            var nullCheckStatement = GetNullCheckStatementForClass(mapInformation);

            var returnStatement =
                ReturnStatement(IdentifierName(returnVariableName))
                    .WithLeadingTrivia(TriviaList(EndOfLine(Environment.NewLine)));

            var statements = new List<StatementSyntax>();

            if (nullCheckStatement != null) statements.Add(nullCheckStatement);
            statements.Add(mappedObjectStatement);
            statements.Add(returnStatement);

            var targetTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(mapInformation.MethodInformation.TargetType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);
            var sourceTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(mapInformation.MethodInformation.SourceType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

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

        private LocalDeclarationStatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto, string returnVariableName, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var propertyToMap in mapInformationDto.PropertiesToMap)
            {
                syntaxNodeOrTokenList.Add(GetPropertyExpression(propertyToMap));
                syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
            }

            var targetTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(mapInformationDto.MethodInformation.TargetType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

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

        private SyntaxNodeOrToken GetPropertyExpression(PropertyToMapDto propertyToMap)
        {
            if (IsSimpleTypeExceptEnum(propertyToMap.Target) ||
                AreArraysOfTheSameSimpleType(propertyToMap.Target, propertyToMap.Source))
            {
                return GetNewDirectConversion(propertyToMap.ParameterName, propertyToMap.Target.Name);
            }

            return GetConversionWithMap(propertyToMap.ParameterName, propertyToMap.Target.Name);
        }

        private static bool IsSimpleTypeExceptEnum(IPropertySymbol target)
        {
            if (target.Type.IsEnum())
            {
                return false;
            }

            return target.Type.IsSimpleType() || target.Type.IsNullableSimpleType();
        }

        private bool AreArraysOfTheSameSimpleType(IPropertySymbol target, IPropertySymbol source)
        {
            return source != null &&
                target.Type.IsArray() && source.Type.IsArray() &&
                SymbolEqualityComparer.Default.Equals(target.Type.GetElementType(), source.Type.GetElementType());
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

    }
}
