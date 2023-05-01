using MapThis.CommonServices.IdentifierNames.Interfaces;
using MapThis.CommonServices.UniqueVariableNames.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.PositionalRecords.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.PositionalRecordMethodGenerators.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.PositionalRecordMethodGenerators
{
    [Export(typeof(IPositionalRecordMethodGenerator))]
    public class PositionalRecordMethodGenerator : IPositionalRecordMethodGenerator
    {
        private readonly IIdentifierNameService IdentifierNameService;
        private readonly IUniqueVariableNameGenerator UniqueVariableNameGenerator;

        [ImportingConstructor]
        public PositionalRecordMethodGenerator(IIdentifierNameService identifierNameService, IUniqueVariableNameGenerator uniqueVariableNameGenerator)
        {
            IdentifierNameService = identifierNameService;
            UniqueVariableNameGenerator = uniqueVariableNameGenerator;
        }

        public MethodDeclarationSyntax Generate(MapInformationForPositionalRecordDto mapInformation, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
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

        private LocalDeclarationStatementSyntax GetMappedObjectStatement(MapInformationForPositionalRecordDto MapInformationForRecordDto, string returnVariableName, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            var isFirstExecution = true;
            foreach (var propertyToMap in MapInformationForRecordDto.PropertiesToMap)
            {
                if (!isFirstExecution)
                {
                    syntaxNodeOrTokenList.Add(Token(SyntaxKind.CommaToken));
                }
                isFirstExecution = false;
                syntaxNodeOrTokenList.Add(GetPropertyExpression(propertyToMap));
            }

            var targetTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(MapInformationForRecordDto.MethodInformation.TargetType, existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

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
                                        ArgumentList(
                                            SeparatedList<ArgumentSyntax>(
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

        private IfStatementSyntax GetNullCheckStatementForClass(MapInformationForPositionalRecordDto MapInformationForRecordDto)
        {
            if (!MapInformationForRecordDto.Options.NullChecking) return null;

            return
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        IdentifierName(MapInformationForRecordDto.MethodInformation.FirstParameterName),
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

        private static ArgumentSyntax GetNewDirectConversion(string identifierName, string propertyName)
        {
            // This will return an expression like "item.Id".
            // If we wanted "Id: item.Id", then we'd need to use WithNameColon.
            return
                Argument(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(identifierName),
                        IdentifierName(propertyName)
                    )
                );
        }

        private static ArgumentSyntax GetConversionWithMap(string identifierName, string propertyName)
        {
            // This will return an expression like "Children: Map(item.Children)".
            // If we wanted "Id: item.Id", then we'd need to use WithNameColon.
            return
                Argument(
                    InvocationExpression(
                        IdentifierName("Map"))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList<ArgumentSyntax>(
                                Argument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(identifierName),
                                        IdentifierName(propertyName)))))));
        }

    }
}
