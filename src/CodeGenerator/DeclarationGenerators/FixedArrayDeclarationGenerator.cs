using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGenerator.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeGenerator.DeclarationGenerators
{
    public readonly struct FixedArrayDeclarationGenerator
    {
        internal StructDeclarationSyntax TryGenerateFixedArrayImplementation(
            StructDeclarationSyntax possibleFixedArrayDeclaration,
            SemanticModel compilation)
        {
            var attributeDataHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
            var attributeRemoveRewriter = RewritersProvider.AttributeRemoveRewriter;

            if (possibleFixedArrayDeclaration.InheritsFrom("IFixedArray"))
            {
                var newMembers = new List<MemberDeclarationSyntax>();
                var fixedArrayDeclaration = possibleFixedArrayDeclaration;
                var attributeList = RoslynDeclarationsGenerator.GenerateAttributes(
                    ("StructLayout", "LayoutKind.Sequential, Pack = 0"),
                    ("System.Runtime.CompilerServices.UnsafeValueType", ""),
                    ("System.Diagnostics.CodeAnalysis.SuppressMessage",
                        @" ""Style"", ""IDE0044:Add readonly modifier"", Justification = ""Required for fixed-size arrays""")
                );

                if (possibleFixedArrayDeclaration.AttributeLists.Count == 0)
                {
                    return default;
                }

                var get = possibleFixedArrayDeclaration.TryGetInheritor("IFixedArray", out var fixedArray);
                var generic = (GenericNameSyntax)fixedArray.Type;

                attributeDataHolder.Update(possibleFixedArrayDeclaration.AttributeLists[0].Attributes, true);
                attributeDataHolder.TryGet("FixedArrayGeneration", out var fixedArrayAttribute2);

                if (fixedArrayAttribute2 == null)
                {
                    return default;
                }

                fixedArrayAttribute2.TryGetFirstArgument(out var maxSize);

                var genericType = generic.GetGenericTypeArgument();
                var maxSizeArrayElementsGeneration =
                    int.Parse(maxSize.Expression.ParseExpressionValue(compilation));

                var namedSymbol = ModelExtensions.GetSymbolInfo(compilation, genericType);
                var typeSymbol = (INamedTypeSymbol)namedSymbol.Symbol;
                var fieldTypeKind = typeSymbol.GetTypedConstantKind();

                var indexer = GenerateIndexer(generic);
                var maxSizeField = GenerateConstMaxSizeField(maxSizeArrayElementsGeneration);
                var listCount = GenerateCurrentCountField();
                var asSpanMethod = GenerateAsSpanMethod(generic);
                var getEnumeratorMethod =
                    RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(RoslynDeclarationsGenerator
                            .GetModifiersList(SyntaxKind.PublicKeyword, SyntaxKind.UnsafeKeyword),
                        "Enumerator",
                        "GetEnumerator"
                    ).AddBodyStatements(
                        $"return new Enumerator(System.Runtime.CompilerServices.Unsafe.AsPointer<{generic.GetGenericTypeArgument().ToFullString()}>(ref _el0));"
                            .ToStatement());
                
                newMembers.Add(maxSizeField);
                newMembers.Add(listCount);

                for (var i = 0; i < maxSizeArrayElementsGeneration; i++)
                {
                    var member =
                        GenerateArrayElementField(generic, i, fieldTypeKind);

                    newMembers.Add(member);
                }

                newMembers.Add(indexer);
                newMembers.Add(asSpanMethod);
                newMembers.Add(getEnumeratorMethod);


                var unsafeEnumerator = GenerateUnsafeEnumerator(generic);


                var getCountProperty = GenerateCountProperty();
                newMembers.Add(getCountProperty);
                newMembers.Add(unsafeEnumerator);
                fixedArrayDeclaration = fixedArrayDeclaration.RemoveBaseType("IFixedArray");
                fixedArrayDeclaration = fixedArrayDeclaration.AddAttributeLists(attributeList);
                fixedArrayDeclaration = fixedArrayDeclaration.AddMembers(newMembers.ToArray());
                fixedArrayDeclaration = (StructDeclarationSyntax)attributeRemoveRewriter.Visit(fixedArrayDeclaration);


                return fixedArrayDeclaration;
            }

            return default;
        }

        private static StructDeclarationSyntax GenerateUnsafeEnumerator(GenericNameSyntax generic)
        {
            var unsafeEnumerator = RoslynDeclarationsGenerator.GenerateEmptyStructDeclaration("Enumerator",
                new[]
                {
                    SyntaxKind.PublicKeyword.ToToken(),
                    SyntaxKind.UnsafeKeyword.ToToken()
                }, null);

            var unsafeEnumeratorConstructor = RoslynDeclarationsGenerator.Constructor(
                    SyntaxKind.PublicKeyword.ToTokenList(),
                    "Enumerator",
                    null,
                    new (string type, string name)[]
                    {
                        ("void*", "ptr")
                    }
                ).AddBodyStatements("_ptr = ptr;".ToStatement(),
                    "_index = -1;".ToStatement())
                .NormalizeWhitespace();
            var currentProperty = RoslynDeclarationsGenerator.GenerateProperty(
                RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.PublicKeyword, SyntaxKind.RefKeyword),
                generic.GetGenericTypeArgument().ToString(),
                "Current",
                SF.AccessorList(SF.SingletonList(RoslynDeclarationsGenerator.GenerateAccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            default,
                            new[]
                            {
                                $"var ptr = System.Runtime.CompilerServices.Unsafe.Add<{generic.GetGenericTypeArgument().ToString()}>(_ptr, _index);"
                                    .ToStatement(),
                                $"return ref System.Runtime.CompilerServices.Unsafe.AsRef<{generic.GetGenericTypeArgument().ToString()}>(ptr);"
                                    .ToStatement()
                            }
                        )
                    )
                )
            );
            var moveNextMethod =
                RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(SyntaxKind.PublicKeyword.ToTokenList(),
                        "bool",
                        "MoveNext"
                    ).AddBodyStatements("return ++_index < MaxSize;".ToStatement())
                    .NormalizeWhitespace();
            var resetMethod = RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                    SyntaxKind.PublicKeyword.ToTokenList(),
                    "void",
                    "Reset"
                ).AddBodyStatements("_index = -1;".ToStatement())
                .NormalizeWhitespace();

            var pointerField = RoslynDeclarationsGenerator.GenerateField(
                RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.PrivateKeyword,
                    SyntaxKind.ReadOnlyKeyword),
                "void*",
                "_ptr");
            var indexField = RoslynDeclarationsGenerator.GenerateField(
                SyntaxKind.PrivateKeyword.ToTokenList(),
                "int",
                "_index");
            unsafeEnumerator = unsafeEnumerator.AddMembers(pointerField,
                indexField,
                unsafeEnumeratorConstructor,
                currentProperty,
                moveNextMethod,
                resetMethod);

            return unsafeEnumerator;
        }

        private static MethodDeclarationSyntax GenerateAsSpanMethod(GenericNameSyntax generic)
        {
            return RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                    RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.PublicKeyword, SyntaxKind.UnsafeKeyword),
                    $"Span<{generic.GetGenericTypeArgument().ToString()}>",
                    "AsSpan")
                .AddBodyStatements(
                    $"return new System.Span<{generic.GetGenericTypeArgument().ToString()}>(System.Runtime.CompilerServices.Unsafe.AsPointer<{generic.GetGenericTypeArgument().ToString()}>(ref _el0), MaxSize);"
                        .ToStatement());
        }

        private static PropertyDeclarationSyntax GenerateCountProperty()
        {
            return RoslynDeclarationsGenerator.GenerateProperty(
                SyntaxKind.PublicKeyword.ToTokenList(),
                "int",
                "Count",
                SF.AccessorList(
                    SF.SingletonList(RoslynDeclarationsGenerator.GenerateAccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        default,
                        new[]
                        {
                            "return CurrentCount;".ToStatement()
                        }
                        //TODO Add attribute
                        //RoslynDeclarationsGenerator.GenerateAttributeListSyntax(RoslynDeclarationsGenerator.GenerateAttributeSyntax("MethodImpl", "MethodImplOptions.AggressiveInlining"))
                    ))));
        }

        private static FieldDeclarationSyntax GenerateArrayElementField(GenericNameSyntax generic, int i,
            SymbolTypeKind fieldTypeKind)
        {
            return RoslynDeclarationsGenerator.GenerateField(
                    SyntaxKind.PrivateKeyword.ToTokenList(),
                    generic.TypeArgumentList.Arguments[0].ToFullString(),
                    $"_el{i}",
                    fieldTypeKind == SymbolTypeKind.Class
                        ? RoslynDeclarationsGenerator.GenerateNewEmptyEqualInitializer(
                            generic.GetGenericTypeArgument().ToString())
                        : null)
                .WithTrailingTrivia(SF.Space)
                .NormalizeWhitespace();
        }

        private static FieldDeclarationSyntax GenerateCurrentCountField()
        {
            return RoslynDeclarationsGenerator.GenerateField(
                    SyntaxKind.PublicKeyword.ToTokenList(),
                    "int",
                    $"CurrentCount"
                )
                .NormalizeWhitespace();
        }

        private static FieldDeclarationSyntax GenerateConstMaxSizeField(int maxSizeArrayElementsGeneration)
        {
            return RoslynDeclarationsGenerator.GenerateField(
                RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.PublicKeyword,
                    SyntaxKind.ConstKeyword),
                "int",
                "MaxSize",
                RoslynDeclarationsGenerator.Integer(maxSizeArrayElementsGeneration)
            );
        }


        private IndexerDeclarationSyntax GenerateIndexer(GenericNameSyntax generic)
        {
            return RoslynDeclarationsGenerator.GenerateIndexer(
                RoslynDeclarationsGenerator.GetModifiersList(
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.UnsafeKeyword,
                    SyntaxKind.RefKeyword),
                generic.TypeArgumentList.Arguments[0].ToFullString(),
                ("int", "index"),
                true,
                SF.TokenList(),
                GenerateUnsafeRefGetterIndexerStatement(generic.TypeArgumentList.Arguments[0].ToFullString())
            ).NormalizeWhitespace();
        }

        private StatementSyntax[] GenerateUnsafeRefGetterIndexerStatement(string fieldType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#IF DEBUG");
            sb.AppendLine("if(index >= MaxSize || index < 0)");
            sb.AppendLine("{");
            sb.AppendLine("throw new IndexOutOfRangeException();");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine($"var ptr = System.Runtime.CompilerServices.Unsafe.AsPointer<{fieldType}>(ref _el0);");
            sb.AppendLine($"var elementPtr = System.Runtime.CompilerServices.Unsafe.Add<{fieldType}>(ptr, index);");
            sb.AppendLine($"return ref System.Runtime.CompilerServices.Unsafe.AsRef<{fieldType}>(elementPtr);");
            return sb.ToString()
                .ToStatements();
        }
    }
}