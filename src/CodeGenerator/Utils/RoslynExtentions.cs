using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeGenerator
{
    internal enum SymbolTypeKind
    {
        Primitive,
        Class,
        Struct,
        Enum,
        Array,
        List,
        Error
    }

    internal static class RoslynExtentions
    {
        internal static bool HaveInterfaceImplementation(this INamedTypeSymbol symbol, string interfaceType)
        {
            if (symbol.Interfaces.Length <= 0)
            {
                return false;
            }

            foreach (var interfaceInfo in symbol.Interfaces)
            {
                if (interfaceInfo.Name.Contains(interfaceType))
                    return true;
            }

            return false;
        }
        
        internal static bool InheritsFrom(this ClassDeclarationSyntax syntax, string type)
        {
            if (syntax.BaseList == null)
            {
                return false;
            }

            if (!syntax.BaseList.Types.Any(a => a.Type.ToString() == type))
            {
                return false;
            }

            return true;
        }
        
        internal static bool InheritsFrom(this StructDeclarationSyntax syntax, string type)
        {
            if (syntax.BaseList == null)
            {
                return false;
            }

            if (!syntax.BaseList.Types.Any(a => a.Type.ToString().Contains(type)))
            {
                return false;
            }

            return true;
        }

        internal static bool TryGetInheritor(this StructDeclarationSyntax syntax, string type, out BaseTypeSyntax typeSyntax)
        {
            typeSyntax = default;
            
            if (syntax.BaseList == null)
            {
                return false;
            }

            typeSyntax = syntax.BaseList.Types.FirstOrDefault(x => x.Type.ToString().Contains(type));
            if (typeSyntax == default)
            {
                return false;
            }
            
            return true;
        }

        internal static bool IsFloatPointType(this ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Decimal:
                    return true;
                default: return false;
            }
        }
        
        internal static SymbolTypeKind GetTypedConstantKind(this ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Byte:
                case SpecialType.System_Decimal:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Char:
                case SpecialType.System_String:
                case SpecialType.System_Object:
                    return SymbolTypeKind.Primitive;
                default:

                    if (type.Name.Contains("List"))
                    {
                        return SymbolTypeKind.List;
                    }

                    switch (type.TypeKind)
                    {
                        case TypeKind.Class:
                            return SymbolTypeKind.Class;
                        case TypeKind.Struct:
                            return SymbolTypeKind.Struct;
                        case TypeKind.Array:
                            return SymbolTypeKind.Array;
                        case TypeKind.Enum:
                            return SymbolTypeKind.Enum;
                        case TypeKind.Error:
                            return SymbolTypeKind.Error;
                    }

                    return SymbolTypeKind.Error;
            }
        }

        internal static string ParseExpressionValue(this ExpressionSyntax expressionSyntax,
            SemanticModel semanticModel)
        {
            return semanticModel.GetConstantValue(expressionSyntax).Value.ToString();
        }


        internal static SyntaxTree ParseFileToSyntaxTree(this string filePath)
        {
            return File.ReadAllText(filePath).ToSyntaxTree();
        }

        internal static SyntaxTree ToSyntaxTree(this string code)
        {
            return CSharpSyntaxTree.ParseText(code);
        }

        internal static string Join(this IEnumerable<string> code, string separator = "")
        {
            return string.Join(separator, code);
        }

        internal static AttributeSyntax ToAttribute(this string code)
        {
            return SF.Attribute(SF.ParseName(code));
        }

        internal static string RemoveEmptySpaces(this string code)
        {
            return Regex.Replace(code, @"\s+", "");
        }

        internal static bool IsList(this GenericNameSyntax genericNameSyntax)
        {
            return genericNameSyntax.Identifier.Text.StartsWith("List");
        }

        internal static FieldDeclarationSyntax WithNewInitializer(this FieldDeclarationSyntax field)
        {
            return SF.FieldDeclaration(
                attributeLists: field.AttributeLists,
                modifiers: field.Modifiers,
                declaration: SF.VariableDeclaration(
                    type: field.Declaration.Type,
                    variables: SF.SingletonSeparatedList(
                        SF.VariableDeclarator(
                            identifier: field.Declaration.Variables[0].Identifier,
                            argumentList: null,
                            initializer: SF.EqualsValueClause(
                                RoslynDeclarationsGenerator.GenerateNewEmptyEqualInitializer(field.FieldTypeName()))
                        )
                    )
                )
            );
        }

        internal static ClassDeclarationSyntax AddInitializationOnClassFields(this ClassDeclarationSyntax node,
            SemanticModel semanticModel)
        {
            var newFields = new List<FieldDeclarationSyntax>();
            var removableFields = new List<FieldDeclarationSyntax>();
            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                var namedSymbol = semanticModel.GetSymbolInfo(field.Declaration.Type);
                var typeSymbol = (INamedTypeSymbol)namedSymbol.Symbol;
                var fieldKind = typeSymbol.GetTypedConstantKind();
                if (fieldKind == SymbolTypeKind.Class)
                {
                    newFields.Add(field.WithNewInitializer());
                    removableFields.Add(field);
                }
            }

            node = node.RemoveNodes(removableFields,
                SyntaxRemoveOptions.KeepTrailingTrivia | SyntaxRemoveOptions.KeepNoTrivia);
            node = node.AddMembers(newFields.ToArray());
            return node;
        }

        internal static StructDeclarationSyntax AddInitializationOnClassFields(this StructDeclarationSyntax node,
            SemanticModel semanticModel)
        {
            var newFields = new List<FieldDeclarationSyntax>();
            var removableFields = new List<FieldDeclarationSyntax>();
            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                var namedSymbol = semanticModel.GetSymbolInfo(field.Declaration.Type);
                var typeSymbol = (INamedTypeSymbol)namedSymbol.Symbol;
                var fieldKind = typeSymbol.GetTypedConstantKind();
                if (fieldKind == SymbolTypeKind.Class)
                {
                    newFields.Add(field.WithNewInitializer());
                    removableFields.Add(field);
                }
            }

            node = node.RemoveNodes(removableFields,
                SyntaxRemoveOptions.KeepTrailingTrivia | SyntaxRemoveOptions.KeepNoTrivia);
            node = node.AddMembers(newFields.ToArray());
            return node;
        }

        internal static TypeSyntax GetGenericTypeArgument(this GenericNameSyntax genericNameSyntax)
        {
            return genericNameSyntax.TypeArgumentList.Arguments.First();
        }

        internal static string FieldName(this BaseFieldDeclarationSyntax fieldDeclarationSyntax)
        {
            return fieldDeclarationSyntax.Declaration.Variables[0].Identifier.Text;
        }

        internal static string FieldTypeName(this BaseFieldDeclarationSyntax fieldDeclarationSyntax)
        {
            return fieldDeclarationSyntax.Declaration.Type.ToString();
        }

        internal static string RemovePartOfString(this string code, string identificator)
        {
            if (string.IsNullOrEmpty(code))
            {
                return code;
            }

            if (!code.Contains(identificator))
            {
                return code;
            }

            var startPos = code.IndexOf(identificator, StringComparison.Ordinal);
            var endPos = identificator.Length;

            return code.Remove(startPos, endPos);
        }

        internal static string JoinWithNewLine(this IEnumerable<string> code)
        {
            return string.Join(Environment.NewLine, code);
        }

        internal static BlockSyntax ToBlock(this string code)
        {
            return SF.Block().AddStatements(code.ToStatements());
        }

        internal static UsingDirectiveSyntax ToUsing(this string usingIdentificator)
        {
            return SF.UsingDirective(SF.ParseName(usingIdentificator));
        }

        internal static UsingDirectiveSyntax[] ToUsings(this string[] usingIdentificator)
        {
            var usingDirectives = new UsingDirectiveSyntax[usingIdentificator.Length];

            for (var i = 0; i < usingDirectives.Length; i++)
            {
                usingDirectives[i] = SF.UsingDirective(SyntaxFactory.ParseName(usingIdentificator[i]));
            }

            return usingDirectives;
        }

        internal static StatementSyntax[] ToStatements(this string code)
        {
            return code
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => SF.ParseStatement(o).NormalizeWhitespace())
                .ToArray();
        }

        internal static StatementSyntax ToStatement(this string code)
        {
            return SF.ParseStatement(code).NormalizeWhitespace();
        }

        internal static NamespaceDeclarationSyntax ToNamespace(this string namespaceName)
        {
            return SF.NamespaceDeclaration(SF.ParseName(namespaceName));
        }

        internal static TypeSyntax ToTypeName(this string typeName)
        {
            return SF.ParseTypeName(typeName);
        }

        internal static IdentifierNameSyntax ToIdentifier(this string identifier)
        {
            return SF.IdentifierName(identifier);
        }

        internal static SyntaxToken ToToken(this SyntaxKind kind)
        {
            return SF.Token(kind);
        }

        internal static SyntaxTokenList ToTokenList(this SyntaxKind kind)
        {
            return SF.TokenList(kind.ToToken());
        }

        internal static SyntaxTokenList ToTokenList(this SyntaxKind[] kinds)
        {
            var list = SF.TokenList();

            return kinds.Aggregate(list, (current, kind) => current.Add(kind.ToToken()));
        }

        internal static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol)
        {
            while (true)
            {
                foreach (var member in symbol.GetMembers())
                {
                    yield return member;
                }

                if (symbol.BaseType == null) yield break;
                {
                    symbol = symbol.BaseType;
                }
            }
        }
    }
}