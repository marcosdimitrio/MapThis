using MapThis.CommonServices.IdentifierNames.Interfaces;
using MapThis.CommonServices.UniqueVariableNames.Interfaces;
using MapThis.Dto;
using MapThis.Helpers;
using MapThis.Services.MappingInformation.MethodConstructors.Constructors.Lists.Dto;
using MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerator.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MapThis.Services.MappingInformation.Services.MethodGenerator.Services.CollectionMethodGenerator
{
    [Export(typeof(ICollectionMethodGeneratorService))]
    public class CollectionMethodGeneratorService : ICollectionMethodGeneratorService
    {
        private readonly IIdentifierNameService IdentifierNameService;
        private readonly IUniqueVariableNameGenerator UniqueVariableNameGenerator;

        [ImportingConstructor]
        public CollectionMethodGeneratorService(IIdentifierNameService identifierNameService, IUniqueVariableNameGenerator uniqueVariableNameGenerator)
        {
            IdentifierNameService = identifierNameService;
            UniqueVariableNameGenerator = uniqueVariableNameGenerator;
        }

        public MethodDeclarationSyntax Generate(MapCollectionInformationDto childMapCollectionInformation, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var mapListStatement = GetMappedListBody(childMapCollectionInformation, codeAnalysisDependenciesDto, existingNamespaces);

            var sourceListTypeName = GetSourceTypeListNameAsInterface(childMapCollectionInformation.MethodInformation.SourceType);

            var targetTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(childMapCollectionInformation.MethodInformation.TargetType.GetElementType(), existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

            var sourceListTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(childMapCollectionInformation.MethodInformation.SourceType.GetElementType(), existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

            var methodDeclaration =
                MethodDeclaration(
                    GenericName(childMapCollectionInformation.MethodInformation.TargetType.Name)
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList(
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
                                        SingletonSeparatedList(
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

        private BlockSyntax GetMappedListBody(MapCollectionInformationDto mapCollectionInformationDto, CodeAnalysisDependenciesDto codeAnalysisDependenciesDto, IList<string> existingNamespaces)
        {
            var destinationVariableName = UniqueVariableNameGenerator.GetUniqueVariableName("destination", mapCollectionInformationDto.MethodInformation.OtherParametersInMethod);

            var targetListTypeSyntax = IdentifierNameService.GetTypeSyntaxConsideringNamespaces(mapCollectionInformationDto.MethodInformation.TargetType.GetElementType(), existingNamespaces, codeAnalysisDependenciesDto.SyntaxGenerator);

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
                                                SingletonSeparatedList(
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

            var forEachVariableName = UniqueVariableNameGenerator.GetUniqueVariableName("item", mapCollectionInformationDto.MethodInformation.OtherParametersInMethod);

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
            if (mapCollectionInformationDto.MethodInformation.SourceType.IsCollectionOfSimpleTypeExceptEnum())
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
