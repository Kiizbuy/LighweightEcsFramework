using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeGenerator
{
    internal enum ConversionOperatorType
    {
        Implicit,
        Explicit
    }

    internal static partial class RoslynDeclarationsGenerator
    {
        internal static SyntaxTree CreateRootSyntaxTree()
        {
            return SF.SyntaxTree(SF.CompilationUnit());
            // return SF.CompilationUnit();
        }
        
        internal static StructDeclarationSyntax ConvertToStructDeclaration(this ClassDeclarationSyntax classDeclaration)
        {
            return SF.StructDeclaration(
                attributeLists: classDeclaration.AttributeLists,
                modifiers: classDeclaration.Modifiers,
                keyword: SyntaxKind.StructKeyword.ToToken(),
                identifier: classDeclaration.Identifier,
                typeParameterList: classDeclaration.TypeParameterList,
                baseList: null,
                constraintClauses: classDeclaration.ConstraintClauses,
                openBraceToken: classDeclaration.OpenBraceToken,
                members: classDeclaration.Members,
                closeBraceToken: classDeclaration.CloseBraceToken,
                semicolonToken: classDeclaration.SemicolonToken);
        }

        internal static ClassDeclarationSyntax ConvertToClassDeclaration(this StructDeclarationSyntax structDeclaration)
        {
            return SF.ClassDeclaration(
                attributeLists: structDeclaration.AttributeLists,
                modifiers: structDeclaration.Modifiers,
                keyword: SyntaxKind.ClassKeyword.ToToken(),
                identifier: structDeclaration.Identifier,
                typeParameterList: structDeclaration.TypeParameterList,
                baseList: structDeclaration.BaseList,
                constraintClauses: structDeclaration.ConstraintClauses,
                openBraceToken: structDeclaration.OpenBraceToken,
                members: structDeclaration.Members,
                closeBraceToken: structDeclaration.CloseBraceToken,
                semicolonToken: structDeclaration.SemicolonToken);
        }

        internal static StructDeclarationSyntax ChangeIdentifierDeclaration(
            this StructDeclarationSyntax declaration,
            string newIdentifier)
        {
            return SF.StructDeclaration(
                attributeLists: declaration.AttributeLists,
                modifiers: declaration.Modifiers,
                keyword: SyntaxKind.StructKeyword.ToToken(),
                identifier: SF.Identifier(newIdentifier),
                typeParameterList: declaration.TypeParameterList,
                baseList: null,
                constraintClauses: declaration.ConstraintClauses,
                openBraceToken: declaration.OpenBraceToken,
                members: declaration.Members,
                closeBraceToken: declaration.CloseBraceToken,
                semicolonToken: declaration.SemicolonToken);
        }

        internal static ClassDeclarationSyntax ChangeIdentifierDeclaration(
            this ClassDeclarationSyntax declaration,
            string newIdentifier)
        {
            return SF.ClassDeclaration(
                attributeLists: declaration.AttributeLists,
                modifiers: declaration.Modifiers,
                keyword: SyntaxKind.ClassKeyword.ToToken(),
                identifier: SF.Identifier(newIdentifier),
                typeParameterList: declaration.TypeParameterList,
                baseList: declaration.BaseList,
                constraintClauses: declaration.ConstraintClauses,
                openBraceToken: declaration.OpenBraceToken,
                members: declaration.Members,
                closeBraceToken: declaration.CloseBraceToken,
                semicolonToken: declaration.SemicolonToken);
        }

        internal static ConstructorDeclarationSyntax ChangeIdentifierDeclaration(
            this ConstructorDeclarationSyntax declaration,
            string newIdentifier)
        {
            return SF.ConstructorDeclaration(
                attributeLists: declaration.AttributeLists,
                modifiers: declaration.Modifiers,
                identifier: SF.Identifier(newIdentifier),
                parameterList: declaration.ParameterList,
                initializer: declaration.Initializer,
                body: declaration.Body);
        }

        
        
        internal static ClassDeclarationSyntax GenerateEmptyClassDeclaration(string name,
            SyntaxToken[] modifierList,
            params string[] inheritanceTypes)
        {
            var classDeclaration = SF.ClassDeclaration(name)
                .AddModifiers(modifierList)
                .NormalizeWhitespace();

            if (inheritanceTypes != null)
                classDeclaration = classDeclaration.AddBaseListTypes(inheritanceTypes.ToInheritanceSyntax());

            return classDeclaration;
        }

        internal static StructDeclarationSyntax GenerateEmptyStructDeclaration(string name,
            SyntaxToken[] modifierList,
            params string[] inheritahceTypes)
        {
            var structDeclarationSyntax = SF.StructDeclaration(name)
                .AddModifiers(modifierList)
                .NormalizeWhitespace();

            if (inheritahceTypes != null)
                structDeclarationSyntax =
                    structDeclarationSyntax.AddBaseListTypes(inheritahceTypes.ToInheritanceSyntax());

            return structDeclarationSyntax;
        }

        internal static AttributeListSyntax GenerateEmptyAttributeDeclarations(params string[] names)
        {
            var attributeList = SF.AttributeList();

            return names.Aggregate(attributeList, (current, name)
                => current.AddAttributes(name.ToAttribute()));
        }

        internal static BaseTypeSyntax[] ToInheritanceSyntax(this string[] inheritanceTypes)
        {
            var baseTypes = new BaseTypeSyntax[inheritanceTypes.Length];

            for (int i = 0; i < baseTypes.Length; i++)
            {
                baseTypes[i] = SF.SimpleBaseType(SF.ParseTypeName(inheritanceTypes[i]));
            }

            return baseTypes;
        }

        internal static SyntaxTrivia GenerateDefineSymbolTrivia(string define)
        {
            return SF.Trivia(SF.DefineDirectiveTrivia(SF.Identifier(define), false));
        }


        internal static SyntaxTrivia GenerateIfDirectiveTrivia(string define)
        {
            return SF.Trivia(SF.IfDirectiveTrivia(SF.ParseExpression(define),
                true,
                true,
                true));
        }

        internal static SyntaxTrivia GenerateEndIfDirectiveTrivia()
        {
            return SF.Trivia(SF.EndIfDirectiveTrivia(true));
        }
        
        internal static AttributeListSyntax GenerateAttributes(params (string attributeName, string argument)[] args)
        {
            var attributes = new List<AttributeSyntax>();
            foreach (var arg in args)
            {
                attributes.Add(GenerateAttributeSyntax(arg.attributeName, arg.argument));
            }

            return GenerateAttributeListSyntax(attributes.ToArray());
        }

        //{ get; set; }
        internal static AccessorListSyntax GetSetAccessorList() =>
            SF.AccessorList(SF.List(new[]
            {
                SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration,
                    default,
                    default,
                    SF.Token(SyntaxKind.GetKeyword),
                    (BlockSyntax) null,
                    SF.Token(SyntaxKind.SemicolonToken)),
                SF.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration,
                    default,
                    default,
                    SF.Token(SyntaxKind.SetKeyword),
                    (BlockSyntax) null,
                    SF.Token(SyntaxKind.SemicolonToken))
            }));

        internal static ConversionOperatorDeclarationSyntax GenerateConversionOperator(
            ConversionOperatorType conversionOperatorType,
            string typeName,
            IEnumerable<(string type, string name)> parameters)
        {
            ParameterListSyntax parameterList = null;

            if (parameters != null)
                parameterList =
                    SF.ParameterList(
                        SF.SeparatedList(GetParametersList(parameters)));

            return SF.ConversionOperatorDeclaration(
                    attributeLists: default,
                    modifiers: GetModifiersList(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword),
                    implicitOrExplicitKeyword: conversionOperatorType == ConversionOperatorType.Implicit
                        ? SyntaxKind.ImplicitKeyword.ToToken()
                        : SyntaxKind.ExplicitKeyword.ToToken(),
                    type: typeName.ToTypeName(),
                    parameterList: parameterList ?? SF.ParameterList(),
                    body: null,
                    expressionBody: null)
                .NormalizeWhitespace();
        }

        internal static ConversionOperatorDeclarationSyntax GenerateConversionOperator(
            ConversionOperatorType conversionOperatorType,
            string typeName,
            params (string type, string name)[] parameters) =>
            GenerateConversionOperator(conversionOperatorType,
                typeName,
                (IEnumerable<(string type, string name)>) parameters);

        internal static SyntaxTokenList GetModifiersList(params SyntaxKind[] keywords)
        {
            var modifiersList = SF.TokenList();
            return keywords.Aggregate(modifiersList, (current, keyword) => current.Add(keyword.ToToken()));
        }

        internal static SyntaxToken[] GetModifiers(params SyntaxKind[] keywords)
        {
            return GetModifiersList(keywords).ToArray();
        }
        

        internal static FieldDeclarationSyntax GenerateField(SyntaxTokenList modifiers,
            string type,
            string identifier,
            ExpressionSyntax initializer = null)
        {
            return Field(modifiers, type.ToTypeName(), Id(identifier), initializer);
        }

        internal static ExpressionSyntax GenerateNewEmptyEqualInitializer(string initializerName)
        {
            return SF.ObjectCreationExpression(
                SF.Token(SyntaxKind.NewKeyword),
                SF.ParseTypeName(initializerName),
                SF.ArgumentList(SF.SeparatedList<ArgumentSyntax>()),
                null
            );
        }


        internal static ExpressionSyntax GenerateAssignmentWithFieldInitializer(string fieldName, string paramName)
        {
            return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                SF.IdentifierName(fieldName),
                SF.IdentifierName(paramName));
        }
        
        internal static LiteralExpressionSyntax Integer(int value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }
        
        internal static LiteralExpressionSyntax Float(float value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.FloatKeyword, SyntaxFactory.Literal(value));
        }
        
        
        internal static EqualsValueClauseSyntax GenerateEqualsValueClause(string value)
        {
            return SF.EqualsValueClause(SF.LiteralExpression(SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(value)));
        }
        
       

        private static FieldDeclarationSyntax Field(SyntaxTokenList modifiers,
            TypeSyntax type,
            SyntaxToken identifier,
            ExpressionSyntax initializer)
        {
            return SF.FieldDeclaration(
                attributeLists: default,
                modifiers: modifiers,
                declaration: VariableDeclaration(type, identifier, initializer));
        }

        internal static ClassDeclarationSyntax AddInheritanceTypeMembers(this ClassDeclarationSyntax type,
            params string[] types)
        {
            type = type.ReplaceNode(type,
                type.AddBaseListTypes(types
                    .Select(baseType => SF.SimpleBaseType(SF.ParseTypeName(baseType)))
                    .Cast<BaseTypeSyntax>()
                    .ToArray()));

            return type;
        }

        internal static StructDeclarationSyntax AddInheritanceTypeMembers(this StructDeclarationSyntax type,
            params string[] types)
        {
            type = type.ReplaceNode(type,
                type.AddBaseListTypes(types
                    .Select(baseType => SF.SimpleBaseType(SF.ParseTypeName(baseType)))
                    .Cast<BaseTypeSyntax>().ToArray()));

            return type;
        }

        
        public static ClassDeclarationSyntax RemoveBaseType(this ClassDeclarationSyntax node, string typeName)
        {
            var baseType = node.BaseList?.Types.FirstOrDefault(x => x.ToString().Contains(typeName));
            if (baseType == null)
            {
                // Base type not found
                return node;
            }

            var baseTypes = node.BaseList!.Types.Remove(baseType);
            if (baseTypes.Count == 0)
            {
                return node
                    .WithBaseList(null)
                    .WithIdentifier(node.Identifier.WithTrailingTrivia(node.BaseList.GetTrailingTrivia()));
            }
            else
            {
                // Remove the type but retain all remaining types and trivia
                return node.WithBaseList(node.BaseList!.WithTypes(baseTypes));
            }
        }
        
        public static StructDeclarationSyntax RemoveBaseType(this StructDeclarationSyntax node, string typeName)
        {
            var baseType = node.BaseList?.Types.FirstOrDefault(x => x.ToString().Contains(typeName));
            if (baseType == null)
            {
                // Base type not found
                return node;
            }

            var baseTypes = node.BaseList!.Types.Remove(baseType);
            if (baseTypes.Count == 0)
            {
                return node
                    .WithBaseList(null)
                    .WithIdentifier(node.Identifier.WithTrailingTrivia(node.BaseList.GetTrailingTrivia()));
            }
            else
            {
                // Remove the type but retain all remaining types and trivia
                return node.WithBaseList(node.BaseList!.WithTypes(baseTypes));
            }
        }

        private static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type,
            SyntaxToken identifier,
            ExpressionSyntax initializer = null)
        {
            return SF.VariableDeclaration(
                type: type,
                variables: SF.SingletonSeparatedList(
                    SF.VariableDeclarator(
                        identifier: identifier,
                        argumentList: null,
                        initializer: initializer == null ? null : SF.EqualsValueClause(initializer))));
        }

        internal static VariableDeclarationSyntax VariableDeclaration(TypeSyntax type,
            string identifier,
            ExpressionSyntax initializer = null)
        {
            return VariableDeclaration(type, Id(identifier), initializer);
        }

        internal static ConstructorDeclarationSyntax Constructor(SyntaxTokenList modifiers,
            string identifier,
            ConstructorInitializerSyntax initializer,
            IEnumerable<(string type, string name)> parameters,
            params StatementSyntax[] statements)
        {
            var parameterList =
                SF.ParameterList(
                    SF.SeparatedList(GetParametersList(parameters)));

            return SF.ConstructorDeclaration(
                attributeLists: default,
                modifiers: modifiers,
                identifier: SF.Identifier(identifier),
                parameterList: parameterList,
                initializer: initializer,
                body: SF.Block(statements));
        }

        internal static ConstructorInitializerSyntax ConstructorInitializer(bool isBase,
            IEnumerable<ExpressionSyntax> argumentExpressions)
        {
            return SF.ConstructorInitializer(
                kind: isBase ? SyntaxKind.BaseConstructorInitializer : SyntaxKind.ThisConstructorInitializer,
                argumentList: ArgumentList(argumentExpressions));
        }

        internal static ConstructorInitializerSyntax ConstructorInitializer(bool isBase,
            params ExpressionSyntax[] argumentExpressions)
        {
            return ConstructorInitializer(isBase, (IEnumerable<ExpressionSyntax>) argumentExpressions);
        }

        internal static SyntaxToken Id(string text)
        {
            return SF.Identifier(default,
                SyntaxKind.IdentifierToken,
                text,
                text.UnescapeIdentifier(),
                default);
        }

        internal static PropertyDeclarationSyntax GenerateProperty(
            SyntaxTokenList modifiers,
            string type,
            string name,
            AccessorListSyntax accessorList,
            IEnumerable<AttributeListSyntax> attributeLists = default)
        {
            return SF.PropertyDeclaration(
                attributeLists: SF.List(attributeLists),
                modifiers: modifiers,
                type: type.ToTypeName(),
                explicitInterfaceSpecifier: null,
                identifier: Id(name),
                accessorList: accessorList,
                expressionBody: null,
                initializer: null);
        }
        
        internal static AttributeSyntax GenerateAttributeSyntax(string attributeName, string argument = "")
        {
            var name = SyntaxFactory.ParseName(attributeName);
            var argumentValue = string.IsNullOrEmpty(argument) ? "" : $"({argument})";
            var arguments = SyntaxFactory.ParseAttributeArgumentList(argumentValue);
            var attribute = SyntaxFactory.Attribute(name, arguments);
            return attribute;
        }

       
        internal static AttributeListSyntax GenerateAttributeListSyntax(params AttributeSyntax[] attributes)
        {
            var attributeList = new SeparatedSyntaxList<AttributeSyntax>();
            attributeList = attributeList.AddRange(attributes);
            var list = SyntaxFactory.AttributeList(attributeList);

            return list;
        }

        internal static PropertyDeclarationSyntax GenerateProperty(
            SyntaxTokenList modifiers,
            string type,
            string name,
            bool getterOnly,
            SyntaxTokenList getterModifiers,
            IEnumerable<StatementSyntax> getterStatements,
            IEnumerable<AttributeListSyntax> attributeLists = default,
            SyntaxTokenList setterModifiers = default,
            IEnumerable<StatementSyntax> setterStatements = null)
        {
            var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, getterModifiers, getterStatements);
            AccessorDeclarationSyntax setter = null;

            if (!getterOnly)
                setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, setterModifiers, setterStatements);

            return GenerateProperty(modifiers, type, name,
                SF.AccessorList(setter == null
                    ? SF.SingletonList(getter)
                    : SF.List(new[] {getter, setter})),
                attributeLists
            );
        }

        internal static PropertyDeclarationSyntax GenerateAutoProperty(SyntaxTokenList modifiers, string type,
            string name)
        {
            return GenerateProperty(
                modifiers,
                type,
                name,
                GetSetAccessorList());
        }

        internal static PropertyDeclarationSyntax GenerateProperty(SyntaxTokenList modifiers, TypeSyntax type,
            SyntaxToken identifier, bool getterOnly,
            SyntaxTokenList getterModifiers, IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default,
            IEnumerable<StatementSyntax> setterStatements = null)
        {
            return GenerateProperty(modifiers, type, identifier, getterOnly, getterModifiers, getterStatements,
                setterModifiers, setterStatements);
        }

        internal static PropertyDeclarationSyntax GenerateProperty(SyntaxTokenList modifiers, TypeSyntax type,
            string identifier, bool getterOnly,
            SyntaxTokenList getterModifiers,
            IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default,
            IEnumerable<StatementSyntax> setterStatements = null)
        {
            return GenerateProperty(modifiers, type, Id(identifier), getterOnly, getterModifiers, getterStatements,
                setterModifiers, setterStatements);
        }

        internal static AccessorDeclarationSyntax AccessorDeclaration(SyntaxKind kind, SyntaxTokenList modifiers,
            IEnumerable<StatementSyntax> statements)
        {
            return SF.AccessorDeclaration(
                kind: kind,
                attributeLists: default,
                modifiers: modifiers,
                body: SF.Block(statements));
        }

        internal static OperatorDeclarationSyntax GenerateEmptyOperatorOverload(SyntaxKind operatorToken,
            string firstTypeName,
            string secondTypeName,
            string firstArgName,
            string secondArgName)
        {
            VerifyOperatorToken(operatorToken);

            var modifiers = GetModifiersList(SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword);
            var firstParamArg = GenerateParameter(firstTypeName, firstArgName);
            var secondParamArg = GenerateParameter(secondTypeName, secondArgName);

            return SF.OperatorDeclaration(
                    firstTypeName.ToTypeName(),
                    operatorToken.ToToken())
                .WithModifiers(modifiers)
                .WithParameterList(SF.ParameterList()
                    .AddParameters(firstParamArg,
                        secondParamArg))
                .NormalizeWhitespace();
        }

        private static void VerifyOperatorToken(SyntaxKind operatorToken)
        {
            switch (operatorToken)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.ExclamationToken:
                case SyntaxKind.TildeToken:
                case SyntaxKind.PlusPlusToken:
                case SyntaxKind.MinusMinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.PercentToken:
                case SyntaxKind.LessThanLessThanToken:
                case SyntaxKind.GreaterThanGreaterThanToken:
                case SyntaxKind.BarToken:
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.CaretToken:
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.ExclamationEqualsToken:
                case SyntaxKind.LessThanToken:
                case SyntaxKind.LessThanEqualsToken:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.GreaterThanEqualsToken:
                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.IsKeyword: break;
                default: throw new ArgumentException(nameof(operatorToken));
            }
        }

        internal static IndexerDeclarationSyntax GenerateIndexer(SyntaxTokenList modifiers,
            string indexerReturnType,
            (string type, string name) parameters,
            bool getterOnly,
            SyntaxTokenList getterModifiers,
            IEnumerable<StatementSyntax> getterStatements,
            SyntaxTokenList setterModifiers = default,
            params StatementSyntax[] setterStatements)
        {
            var paramList = SF.ParameterList(
                SF.SeparatedList(GetParametersList(new[]
                {
                    parameters
                })));


            var getter =
                GenerateAccessorDeclaration(SyntaxKind.GetAccessorDeclaration, getterModifiers, getterStatements);
            AccessorDeclarationSyntax setter = null;
            if (!getterOnly)
                setter = GenerateAccessorDeclaration(SyntaxKind.SetAccessorDeclaration, setterModifiers,
                    setterStatements);

            return SF.IndexerDeclaration(
                attributeLists: default,
                modifiers: modifiers,
                type: SF.ParseName(indexerReturnType),
                explicitInterfaceSpecifier: null,
                parameterList: BracketedParameterList(paramList),
                accessorList: SF.AccessorList(setter == null
                    ? SF.SingletonList(getter)
                    : SF.List(new[] {getter, setter})));
        }


        internal static AccessorDeclarationSyntax GenerateAccessorDeclaration(SyntaxKind kind,
            SyntaxTokenList modifiers,
            IEnumerable<StatementSyntax> statements,
            SyntaxList<AttributeListSyntax> attributes = default)
        {
            return SF.AccessorDeclaration(
                kind: kind,
                attributeLists: attributes,
                modifiers: modifiers,
                body: SF.Block(statements));
        }

        internal static ParameterSyntax Parameter(SyntaxTokenList modifiers,
            TypeSyntax type,
            SyntaxToken identifier,
            ExpressionSyntax @default = null)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: modifiers,
                type: type,
                identifier: identifier,
                @default: @default == null ? null : SF.EqualsValueClause(@default));
        }

        internal static ParameterSyntax Parameter(SyntaxTokenList modifiers,
            string type,
            string identifier,
            ExpressionSyntax @default = null)
        {
            return Parameter(modifiers, type.ToTypeName(), Id(identifier), @default);
        }

        internal static string UnescapeIdentifier(this string identifier)
        {
            return identifier[0] == '@' ? identifier.Substring(1) : identifier;
        }

        internal static ParameterSyntax GenerateParameter(string type,
            string identifier,
            ExpressionSyntax @default = null)
        {
            return Parameter(default, type, identifier, @default);
        }

        internal static ParameterSyntax Parameter(string identifier)
        {
            return GenerateParameter(null, identifier);
        }

        internal static BracketedParameterListSyntax BracketedParameterList(ParameterListSyntax parameters)
        {
            return SF.BracketedParameterList(parameters.Parameters);
        }

        internal static BracketedParameterListSyntax BracketedParameterList(IEnumerable<ParameterSyntax> parameters)
        {
            return SF.BracketedParameterList(SF.SeparatedList(parameters));
        }

        internal static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> parameters)
        {
            return SF.ParameterList(SF.SeparatedList(parameters));
        }

        internal static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters)
        {
            return ParameterList((IEnumerable<ParameterSyntax>) parameters);
        }

        //>(..) => body
        internal static ParenthesizedLambdaExpressionSyntax ParedLambdaExpression(
            IEnumerable<ParameterSyntax> parameters,
            CSharpSyntaxNode body)
        {
            return SF.ParenthesizedLambdaExpression(ParameterList(parameters), body);
        }

        //>para => body
        internal static SimpleLambdaExpressionSyntax SimpleLambdaExpression(string parameter, CSharpSyntaxNode body)
        {
            return SF.SimpleLambdaExpression(Parameter(parameter), body);
        }

        //>para => { ... }
        internal static SimpleLambdaExpressionSyntax SimpleLambdaExpression(string parameter,
            IEnumerable<StatementSyntax> statements)
        {
            return SimpleLambdaExpression(parameter, SF.Block(statements));
        }

        internal static MethodDeclarationSyntax GenerateEmptyMethodDeclaration(SyntaxTokenList methodKeywords,
            string returnTypeName,
            string methodName,
            params (string type, string name)[] parameters)
        {
            ParameterListSyntax parameterList = null;

            if (parameters != null)
                parameterList =
                    SF.ParameterList(
                        SF.SeparatedList(GetParametersList(parameters)));


            return SF.MethodDeclaration(attributeLists: SF.List<AttributeListSyntax>(),
                    modifiers: methodKeywords,
                    returnType: returnTypeName.ToTypeName(),
                    explicitInterfaceSpecifier: null,
                    identifier: Id(methodName),
                    typeParameterList: null,
                    parameterList: parameterList ?? SF.ParameterList(),
                    constraintClauses: SF.List<TypeParameterConstraintClauseSyntax>(),
                    body: null,
                    semicolonToken: default)
                .NormalizeWhitespace();
        }

        internal static ArgumentListSyntax ArgumentList(IEnumerable<ArgumentSyntax> arguments)
        {
            return SF.ArgumentList(SF.SeparatedList(arguments));
        }

        internal static ArgumentListSyntax ArgumentList(params ArgumentSyntax[] arguments)
        {
            return ArgumentList((IEnumerable<ArgumentSyntax>) arguments);
        }

        internal static ArgumentListSyntax ArgumentList(IEnumerable<ExpressionSyntax> argExprs)
        {
            return ArgumentList(argExprs == null ? null : argExprs.Select(i => SF.Argument(i)));
        }

        internal static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] argExprs)
        {
            return ArgumentList((IEnumerable<ExpressionSyntax>) argExprs);
        }

        internal static IEnumerable<ParameterSyntax> GetParametersList(
            IEnumerable<(string type, string name)> parameters)
        {
            return parameters.Select(type2name => SF.Parameter(
                attributeLists: SF.List<AttributeListSyntax>(),
                modifiers: SF.TokenList(),
                type: SF.ParseTypeName(type2name.type),
                identifier: SF.Identifier(string.IsNullOrEmpty(type2name.name)
                    ? string.Empty
                    : type2name.name),
                @default: null));
        }
    }
}