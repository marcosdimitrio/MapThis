using MapThis.Dto;
using MapThis.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
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
            var mapInformations = await GetMapInformations(context, methodSyntax, cancellationToken).ConfigureAwait(false);

            var blocks = new List<MethodDeclarationSyntax>();

            var firstStatement = GetMappedObjectStatement(mapInformations.First());

            // first
            var firstBlockSyntax = methodSyntax
                .WithBody(Block(
                    firstStatement,
                    EmptyStatement()
                        .WithSemicolonToken(
                            MissingToken(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(CarriageReturnLineFeed, CarriageReturnLineFeed, Whitespace(new string(' ', methodSyntax.GetLeadingTrivia().FullSpan.Length + 4))))
                        ),
                    ReturnStatement(IdentifierName("newItem"))
                ));
            //    .WithTriviaF‌​rom(methodSyntax)
            //    .W‌​ithAdditionalAnnotat‌​ions(Formatter.Annot‌​ation);

            blocks.Add(firstBlockSyntax);

            foreach (var mapInformation in mapInformations.Skip(1))
            {
                var statement = GetMappedObjectStatement(mapInformation);

                var blockSyntax =
                    MethodDeclaration(
                        IdentifierName(mapInformation.TargetType.Name),
                        Identifier("Map")
                    )
                    .WithModifiers(
                        TokenList(Token(SyntaxKind.PrivateKeyword))
                    )
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList<ParameterSyntax>(
                                Parameter(Identifier("item"))
                                    .WithType(IdentifierName(mapInformation.SourceType.Name))
                            )
                        )
                    )
                    .WithBody(
                        Block(
                            statement,
                            EmptyStatement()
                                .WithSemicolonToken(
                                    MissingToken(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(CarriageReturnLineFeed, CarriageReturnLineFeed, Whitespace(new string(' ', methodSyntax.GetLeadingTrivia().FullSpan.Length + 4))))
                                ),
                            ReturnStatement(IdentifierName("newItem"))
                        )
                    );

                blocks.Add(blockSyntax);
            }

            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(methodSyntax, blocks);
            return context.Document.WithSyntaxRoot(newRoot);
        }

        private static async Task<IList<MapInformationDto>> GetMapInformations(CodeRefactoringContext context, MethodDeclarationSyntax methodSyntax, CancellationToken cancellationToken)
        {
            var semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken);
            //var generator = SyntaxGenerator.GetGenerator(context.Document);

            var firstParameterSymbol = methodSymbol.Parameters[0];
            var targetType = methodSymbol.ReturnType;

            var sourceMembers = firstParameterSymbol.Type.GetMembers()
                .Where(x => x.Kind == SymbolKind.Property && x.DeclaredAccessibility == Accessibility.Public)
                .Cast<IPropertySymbol>()
                .ToList();
            var targetMembers = targetType.GetMembers()
                .Where(x => x.Kind == SymbolKind.Property && x.DeclaredAccessibility == Accessibility.Public)
                .Cast<IPropertySymbol>()
                .ToList();

            var mapInformations = GetMaps(firstParameterSymbol.Type, targetType, firstParameterSymbol.Name, sourceMembers, targetMembers);

            return mapInformations;
        }

        private static IList<MapInformationDto> GetMaps(ITypeSymbol sourceType, ITypeSymbol targetType, string firstParameterName, List<IPropertySymbol> sourceMembers, List<IPropertySymbol> targetMembers)
        {
            var mapInformations = new List<MapInformationDto>();

            var propertiesToMap = new List<PropertyToMapDto>();

            foreach (var targetProperty in targetMembers)
            {
                var sourceProperty = FindPropertyInSource(targetProperty, sourceMembers);

                SyntaxNode newExpression = null;
                INamedTypeSymbol targetListType = null;
                INamedTypeSymbol sourceListType = null;

                if (targetProperty.Type.IsSimpleType())
                {
                    newExpression = GetNewDirectConversion(firstParameterName, targetProperty.Name);
                }
                else
                {
                    if (
                        targetProperty.Type is INamedTypeSymbol targetNamedType && targetNamedType.IsGenericType &&
                        sourceProperty?.Type is INamedTypeSymbol sourceNamedType && sourceNamedType.IsGenericType
                    )
                    {
                        // use namedType.TypeArguments if type is bound generic or namedType.TypeParameters if isn't
                        targetListType = (INamedTypeSymbol)targetNamedType.TypeArguments[0];
                        sourceListType = (INamedTypeSymbol)sourceNamedType.TypeArguments[0];

                        var sourceListMembers = sourceListType.GetMembers()
                            .Where(x => x.Kind == SymbolKind.Property && x.DeclaredAccessibility == Accessibility.Public)
                            .Cast<IPropertySymbol>()
                            .ToList();
                        var targetListMembers = targetListType.GetMembers()
                            .Where(x => x.Kind == SymbolKind.Property && x.DeclaredAccessibility == Accessibility.Public)
                            .Cast<IPropertySymbol>()
                            .ToList();

                        var childMapInformation = GetMaps(sourceListType, targetListType, "item", sourceListMembers, targetListMembers);
                        mapInformations.AddRange(childMapInformation);
                    }
                    else if (
                        targetProperty.Type is IArrayTypeSymbol targetArrayType &&
                        sourceProperty?.Type is IArrayTypeSymbol sourceArrayType
                    )
                    {
                        targetListType = (INamedTypeSymbol)targetArrayType.ElementType;
                        sourceListType = (INamedTypeSymbol)sourceArrayType.ElementType;

                        throw new NotImplementedException();
                    }

                    newExpression = GetConversionWithMap(firstParameterName, targetProperty.Name);
                }

                var propertyToMap = new PropertyToMapDto(sourceProperty, targetProperty, newExpression);

                propertiesToMap.Add(propertyToMap);
            }

            mapInformations.Add(new MapInformationDto(firstParameterName, propertiesToMap, sourceType, targetType));

            mapInformations.Reverse();

            return mapInformations;
        }

        private StatementSyntax GetMappedObjectStatement(MapInformationDto mapInformationDto)
        {
            var syntaxNodeOrTokenList = new List<SyntaxNodeOrToken>();

            foreach (var propertyToMap in mapInformationDto.PropertiesToMap)
            {
                AssignmentExpressionSyntax assignment;

                if (propertyToMap.Target.Type.IsSimpleType())
                {
                    assignment = GetDirectAssignment(propertyToMap.TargetName, mapInformationDto.FirstParameterName, propertyToMap.SourceName);
                }
                else
                {
                    assignment = GetAssignmentWithMap(propertyToMap.TargetName, mapInformationDto.FirstParameterName, propertyToMap.SourceName);
                }

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

        private static AssignmentExpressionSyntax GetDirectAssignment(string leftIdentifierName, string parameterName, string rightIdentifierName)
        {
            // This will return an expression like "Id = item.Id"
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

        private static AssignmentExpressionSyntax GetAssignmentWithMap(string leftIdentifierName, string parameterName, string rightIdentifierName)
        {
            // This will return an expression like "Children = Map(item.Children)"
            return
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(leftIdentifierName),
                    InvocationExpression(
                        IdentifierName("Map")
                    )
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(parameterName),
                                        IdentifierName(rightIdentifierName)
                                    )
                                )
                            )
                        )
                    )
                )
                .WithLeadingTrivia(ElasticCarriageReturnLineFeed);
        }

        private StatementSyntax MyTest2(SyntaxNode[] mappingExpressions, int triviaLength)
        {
            return
                LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName(
                            Identifier(
                                TriviaList(
                                    Whitespace("            ")),
                                SyntaxKind.VarKeyword,
                                "var",
                                "var",
                                TriviaList(
                                    Space))))
                    .WithVariables(
                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                            VariableDeclarator(
                                Identifier(
                                    TriviaList(),
                                    "newItem",
                                    TriviaList(
                                        Space)))
                            .WithInitializer(
                                EqualsValueClause(
                                    ObjectCreationExpression(
                                        IdentifierName("Parent"))
                                    .WithNewKeyword(
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.NewKeyword,
                                            TriviaList(
                                                Space)))
                                    .WithArgumentList(
                                        ArgumentList()
                                        .WithCloseParenToken(
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CloseParenToken,
                                                TriviaList(
                                                    LineFeed))))
                                    .WithInitializer(
                                        InitializerExpression(
                                            SyntaxKind.ObjectInitializerExpression,
                                            SeparatedList<ExpressionSyntax>(
                                                new SyntaxNodeOrToken[]{
                                                    AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        IdentifierName(
                                                            Identifier(
                                                                TriviaList(
                                                                    Whitespace("                ")),
                                                                "Id",
                                                                TriviaList(
                                                                    Space))),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("item"),
                                                            IdentifierName("Id")))
                                                    .WithOperatorToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.EqualsToken,
                                                            TriviaList(
                                                                Space))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.CommaToken,
                                                        TriviaList(
                                                            LineFeed)),
                                                    AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        IdentifierName(
                                                            Identifier(
                                                                TriviaList(
                                                                    Whitespace("                ")),
                                                                "DataHora",
                                                                TriviaList(
                                                                    Space))),
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("item"),
                                                            IdentifierName("DataHora")))
                                                    .WithOperatorToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.EqualsToken,
                                                            TriviaList(
                                                                Space))),
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.CommaToken,
                                                        TriviaList(
                                                            LineFeed)),
                                                    AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        IdentifierName(
                                                            Identifier(
                                                                TriviaList(
                                                                    Whitespace("                ")),
                                                                "Children",
                                                                TriviaList(
                                                                    Space))),
                                                        InvocationExpression(
                                                            IdentifierName("Map"))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList<ArgumentSyntax>(
                                                                    Argument(
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("item"),
                                                                            IdentifierName("Children")))))
                                                            .WithCloseParenToken(
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CloseParenToken,
                                                                    TriviaList(
                                                                        LineFeed)))))
                                                    .WithOperatorToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.EqualsToken,
                                                            TriviaList(
                                                                Space)))}))
                                        .WithOpenBraceToken(
                                            Token(
                                                TriviaList(
                                                    Whitespace("            ")),
                                                SyntaxKind.OpenBraceToken,
                                                TriviaList(
                                                    LineFeed)))
                                        .WithCloseBraceToken(
                                            Token(
                                                TriviaList(
                                                    Whitespace("            ")),
                                                SyntaxKind.CloseBraceToken,
                                                TriviaList()))))
                                .WithEqualsToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.EqualsToken,
                                        TriviaList(
                                            Space)))))))
                .WithSemicolonToken(
                    Token(
                        TriviaList(),
                        SyntaxKind.SemicolonToken,
                        TriviaList(
                            new[]{
                                LineFeed,
                                LineFeed,
                                Whitespace("            "),
                                Trivia(
                                    SkippedTokensTrivia()
                                    .WithTokens(
                                        TokenList(
                                            Token(SyntaxKind.ReturnKeyword)))),
                                Space,
                                Trivia(
                                    SkippedTokensTrivia()
                                    .WithTokens(
                                        TokenList(
                                            Identifier("newItem")))),
                                Trivia(
                                    SkippedTokensTrivia()
                                    .WithTokens(
                                        TokenList(
                                            Token(SyntaxKind.SemicolonToken))))})));
        }

        private static IPropertySymbol FindPropertyInSource(IPropertySymbol targetProperty, IList<IPropertySymbol> sourceMembers)
        {
            var sourceProperty = sourceMembers.FirstOrDefault(x => x.Name == targetProperty.Name);

            return sourceProperty;
        }

        private static ExpressionSyntax GetNewDirectConversion(string identifierName, string propertyName)
        {
            return
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(propertyName),
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(identifierName),
                        IdentifierName(propertyName)))
                .NormalizeWhitespace();
        }

        private static ExpressionSyntax GetConversionWithMap(string identifierName, string propertyName)
        {
            return AssignmentExpression(
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
                .NormalizeWhitespace();
        }

        private async Task<Solution> ReverseTypeNameAsync(Document document, MethodDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Produce a reversed version of the type declaration's identifier token.
            var identifierToken = typeDecl.Identifier;
            var newName = new string(identifierToken.Text.ToCharArray().Reverse().ToArray());

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}
