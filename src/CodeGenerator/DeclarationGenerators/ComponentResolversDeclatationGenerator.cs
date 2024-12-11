using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGenerator.Rewriters;
using CodeGenerator.TestDatas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.DeclarationGenerators
{
    public struct ComponentResolversDeclatationGenerator
    {
        internal ClassDeclarationSyntax GenerateComponentResolverImplementation(
            StructDeclarationSyntax component,
            SemanticModel semanticModel)
        {
            var attributeFieldRewriter = RewritersProvider.AttributeRemoveRewriter;
            var componentFields = component
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .ToArray();

            var emptyComponent = (StructDeclarationSyntax)attributeFieldRewriter.Visit(component);
            var emptyFields = emptyComponent
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .ToArray();

            var componentName = component.Identifier.Text;
            var componentResolverName = $"{componentName}ComponentResolver";
            var componentResolverInheritance = $"ComponentResolver<{componentName}, {componentResolverName}.Data>";

            var componentResolver = RoslynDeclarationsGenerator.GenerateEmptyClassDeclaration(
                $"{componentResolverName}",
                new[]
                {
                    SyntaxKind.PublicKeyword.ToToken(),
                    SyntaxKind.SealedKeyword.ToToken()
                },
                componentResolverInheritance
            );

            var serializableStruct = RoslynDeclarationsGenerator.GenerateEmptyStructDeclaration("Data",
                new[]
                {
                    SyntaxKind.PublicKeyword.ToToken()
                }, "ISerializableData");
            serializableStruct = serializableStruct.AddMembers(emptyFields)
                .NormalizeWhitespace();

            var dataSerializeMethod = GenerateSerializeMethod(semanticModel, componentFields);
            var dataDeserializeMethod = GenerateDeserializeMethod(semanticModel, componentFields);


            serializableStruct = serializableStruct.AddMembers(dataSerializeMethod, dataDeserializeMethod);
            var fillSerializableDataFromMethod = GenerateFillSerializableDataFromMethod(componentName, emptyFields);
            var fillComponentMethod = GenerateFillComponentMethod(componentName, emptyFields);

            componentResolver = componentResolver.AddMembers(serializableStruct,
                fillSerializableDataFromMethod,
                fillComponentMethod);

            return componentResolver;
        }

        private MethodDeclarationSyntax GenerateFillComponentMethod(string componentName,
            FieldDeclarationSyntax[] emptyFields)
        {
            return RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                    RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.ProtectedKeyword,
                        SyntaxKind.OverrideKeyword),
                    "void",
                    "FillComponent",
                    ("ref Data", "data"),
                    ($"ref {componentName}", "component")
                ).AddBodyStatements(FillComponentDataFromStatement(emptyFields).ToStatements())
                .NormalizeWhitespace();
        }

        private MethodDeclarationSyntax GenerateFillSerializableDataFromMethod(string componentName,
            FieldDeclarationSyntax[] emptyFields)
        {
            return RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                    RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.ProtectedKeyword,
                        SyntaxKind.OverrideKeyword),
                    "Data",
                    "FillSerializableDataFrom",
                    ($"ref {componentName}", "component")
                ).AddBodyStatements(FillSerializableDataFromStatement(emptyFields).ToStatements())
                .NormalizeWhitespace();
        }

        private MethodDeclarationSyntax GenerateDeserializeMethod(SemanticModel semanticModel,
            FieldDeclarationSyntax[] componentFields)
        {
            return RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                    SyntaxKind.PublicKeyword.ToTokenList(),
                    "void",
                    "Deserialize",
                    ("ISerializePacker", "packer")
                ).AddBodyStatements(DeserializeFields(componentFields, semanticModel).ToStatements())
                .NormalizeWhitespace();
        }

        private MethodDeclarationSyntax GenerateSerializeMethod(SemanticModel semanticModel,
            FieldDeclarationSyntax[] componentFields)
        {
            return RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                    SyntaxKind.PublicKeyword.ToTokenList(),
                    "void",
                    "Serialize",
                    ("ISerializePacker", "packer")
                ).AddBodyStatements(SerializeFields(componentFields, semanticModel).ToStatements())
                .NormalizeWhitespace();
        }

        private string FillSerializableDataFromStatement(IEnumerable<FieldDeclarationSyntax> fields)
        {
            var sb = new StringBuilder();
            var attributeHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
            attributeHolder.Clear();
            sb.AppendLine("Data data;");

            foreach (var field in fields)
            {
                if (field.AttributeLists.Count > 0)
                {
                    foreach (var attribute in field.AttributeLists)
                    {
                        attributeHolder.Update(attribute.Attributes);
                    }
                }

                if (attributeHolder.TryGet("IgnoreSerialization", out _) == false)
                {
                    sb.AppendLine($"data.{field.FieldName()} = component.{field.FieldName()};");
                }
            }

            sb.AppendLine("return data;");
            return sb.ToString();
        }

        private string FillComponentDataFromStatement(IEnumerable<FieldDeclarationSyntax> fields)
        {
            var sb = new StringBuilder();
            var attributeHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
            attributeHolder.Clear();
            foreach (var field in fields)
            {
                if (field.AttributeLists.Count > 0)
                {
                    foreach (var attribute in field.AttributeLists)
                    {
                        attributeHolder.Update(attribute.Attributes);
                    }
                }

                if (attributeHolder.TryGet("IgnoreSerialization", out _) == false)
                {
                    sb.AppendLine($"component.{field.FieldName()} = data.{field.FieldName()};");
                }
            }

            return sb.ToString();
        }


        private string SerializeFields(IEnumerable<FieldDeclarationSyntax> fields, SemanticModel compilation)
        {
            var sb = new StringBuilder();

            foreach (var field in fields)
            {
                sb.AppendLine(SerializeField(field, compilation));
            }

            return sb.ToString();
        }

        public class MinMaxData
        {
            public string MinValue;
            public string MaxValue;
            public string Precision;
        }

        internal string SerializeField(BaseFieldDeclarationSyntax field,
            SemanticModel compilation,
            bool useMask = false)
        {
            MinMaxData minMaxAttributeData = null;
            var namedSymbol = compilation.GetSymbolInfo(field.Declaration.Type);
            var typeSymbol = (INamedTypeSymbol)namedSymbol.Symbol;
            var fieldTypeKind = typeSymbol.GetTypedConstantKind();
            var enumUnderlyingType = typeSymbol.TypeKind == TypeKind.Enum
                ? typeSymbol.EnumUnderlyingType.ToString()
                : string.Empty;

            var sb = new StringBuilder();
            var fieldName = field.FieldName();

            if (field.AttributeLists.Count > 0)
            {
                var attributeDataHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
                attributeDataHolder.Clear();
                foreach (var attributes in field.AttributeLists)
                {
                    attributeDataHolder.Update(attributes.Attributes);
                }

                if (attributeDataHolder.TryGet("IgnoreSerialization", out _))
                {
                    attributeDataHolder.Clear();
                    return string.Empty;
                }

                if (attributeDataHolder.TryGet("MinMaxValue", out var minMaxData))
                {
                    minMaxAttributeData = new MinMaxData()
                    {
                        MinValue = minMaxData.GetArgumentViaIndex(0).Expression.ParseExpressionValue(compilation),
                        MaxValue = minMaxData.GetArgumentViaIndex(1).Expression.ParseExpressionValue(compilation),
                    };
                }

                if (attributeDataHolder.TryGet("FloatMinMaxValue", out minMaxData))
                {
                    minMaxAttributeData = new MinMaxData()
                    {
                        MinValue = minMaxData.GetArgumentViaIndex(0).Expression.ToFullString(),
                        MaxValue = minMaxData.GetArgumentViaIndex(1).Expression.ToFullString(),
                        Precision = minMaxData.GetArgumentViaIndex(2).Expression.ToFullString()
                    };
                }
            }

            {
                sb.AppendLine(SerializeValue(field,
                    fieldName,
                    fieldTypeKind,
                    enumUnderlyingType,
                    minMaxAttributeData)
                );
            }

            return sb.ToString();
        }

        internal string SerializeValue(BaseFieldDeclarationSyntax field,
            string name,
            SymbolTypeKind fieldTypeKindKind,
            string enumUnderlyingType,
            MinMaxData minMaxData)
        {
            var sb = new StringBuilder();
            var attributeDataHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
            var genericField = field.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
            var isEnum = fieldTypeKindKind == SymbolTypeKind.Enum;
            var isList = genericField != null && genericField.IsList();
            var fieldType = isList
                ? genericField.GetGenericTypeArgument().ToString()
                : field.FieldTypeName();

            attributeDataHolder.Clear();

            if (SizeAndBitsCountUtils.IsPackerSerializableType(fieldType) == false && isEnum == false)
            {
                sb.AppendLine($"{name}.Serialize(packer);");
                return sb.ToString();
            }

            if (isEnum)
            {
                if (string.IsNullOrEmpty(enumUnderlyingType) == false)
                {
                    var enumSize = SizeAndBitsCountUtils.GetSizeAndBitsCountText(enumUnderlyingType);
                    var bits = (enumSize.Value > 0) ? "" : $", {enumSize.Value}";
                    sb.AppendLine(
                        $"packer.Write({name}, {bits});\n");
                }

                return sb.ToString();
            }


            var pair = SizeAndBitsCountUtils.GetSizeAndBitsCountText(fieldType);
            var hasSize = string.IsNullOrEmpty(pair.Key) == false && pair.Value > 0;
            if (minMaxData != null)
            {
                var precisionIsNotNull = string.IsNullOrEmpty(minMaxData.Precision) == false;
                if (precisionIsNotNull)
                {
                    sb.AppendLine(
                        $"packer.Write({name}, {minMaxData.MinValue}, {minMaxData.MaxValue}, {minMaxData.Precision});");
                }
                else
                {
                    sb.AppendLine($"packer.Write({name}, {minMaxData.MinValue}, {minMaxData.MaxValue});");
                }
            }
            else
            {
                {
                    sb.AppendLine(
                        $"packer.Write({name}{(hasSize ? $", {pair.Value}" : "")});\n");
                }
            }

            return sb.ToString();
        }

        private string DeserializeFields(IEnumerable<BaseFieldDeclarationSyntax> fields,
            SemanticModel compilation,
            bool useMask = false)
        {
            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                sb.AppendLine(DeserializeField(field, compilation, useMask));
            }

            return sb.ToString();
        }

        internal string DeserializeField(BaseFieldDeclarationSyntax field,
            SemanticModel compilation,
            bool useMask = false)
        {
            MinMaxData minMaxAttributeData = null;
            var sb = new StringBuilder();
            var attributeDataHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
            var fieldName = field.FieldName();
            var namedSymbol = compilation.GetSymbolInfo(field.Declaration.Type);
            var typeSymbol = (INamedTypeSymbol)namedSymbol.Symbol;
            var fieldTypeKind = typeSymbol.GetTypedConstantKind();
            var enumUnderlyingType = typeSymbol.TypeKind == TypeKind.Enum
                ? typeSymbol.EnumUnderlyingType.ToString()
                : string.Empty;
            var isFixedList = typeSymbol.HaveInterfaceImplementation("IFixedArray");
            var fixedListMetaData = typeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.SyntaxTree.GetRoot();
            attributeDataHolder.Clear();

            if (field.AttributeLists.Count > 0)
            {
                if (field.AttributeLists.Count > 0)
                {
                    foreach (var attribute in field.AttributeLists)
                    {
                        attributeDataHolder.Update(attribute.Attributes);
                    }
                }

                if (attributeDataHolder.TryGet("IgnoreSerialization", out _))
                {
                    return string.Empty;
                }

                if (attributeDataHolder.TryGet("MinMaxValue", out var minMaxData))
                {
                    minMaxAttributeData = new MinMaxData()
                    {
                        MinValue = minMaxData.GetArgumentViaIndex(0).Expression.ParseExpressionValue(compilation),
                        MaxValue = minMaxData.GetArgumentViaIndex(1).Expression.ParseExpressionValue(compilation),
                    };
                }

                if (attributeDataHolder.TryGet("FloatMinMaxValue", out minMaxData))
                {
                    minMaxAttributeData = new MinMaxData()
                    {
                        MinValue = minMaxData.GetArgumentViaIndex(0).Expression.ToFullString(),
                        MaxValue = minMaxData.GetArgumentViaIndex(1).Expression.ToFullString(),
                        Precision = minMaxData.GetArgumentViaIndex(2).Expression.ToFullString()
                    };
                }
            }

            sb.AppendLine(DeserializeValue(field,
                fieldName,
                fieldTypeKind,
                enumUnderlyingType,
                minMaxAttributeData
            ));

            return sb.ToString();
        }

        private string DeserializeValue(BaseFieldDeclarationSyntax field,
            string name,
            SymbolTypeKind fieldTypeKind,
            string enumUnderlyingType,
            MinMaxData minMax = null)
        {
            var sb = new StringBuilder();
            var genericField = field.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault();
            var attributeDataHolder = RoslynAttributeDataHolder.GetOrCreateInstance.Value;
            var isValueType = fieldTypeKind == SymbolTypeKind.Struct;
            var isEnum = fieldTypeKind == SymbolTypeKind.Enum;
            var isList = genericField != null && genericField.IsList();
            var fieldType = isList
                ? genericField.GetGenericTypeArgument().ToString()
                : field.FieldTypeName();

            attributeDataHolder.Clear();

            if (SizeAndBitsCountUtils.IsPackerSerializableType(fieldType) == false && isEnum == false)
            {
                if (isValueType)
                {
                    sb.AppendLine($"var codegenTemp{fieldType} = new {fieldType}();");
                    sb.AppendLine($"codegenTemp{fieldType}.Deserialize(packer);");
                    sb.AppendLine($"{name} = codegenTemp{fieldType};");
                }
                else
                {
                    sb.AppendLine(($"{name}.Deser(packer);"));
                }

                return sb.ToString();
            }
            else
            {
                var pair = SizeAndBitsCountUtils.GetSizeAndBitsCountText(isEnum
                    ? enumUnderlyingType
                    : fieldType);

                var hasSize = string.IsNullOrEmpty(pair.Key) == false && pair.Value > 0;


                if (minMax != null)
                {
                    var precisionValue = string.IsNullOrEmpty(minMax.Precision) ? "" : $", {minMax.Precision}";
                    sb.AppendLine(
                        $"{name} = packer.Read{pair.Key}({minMax.MinValue}, {minMax.MaxValue}{precisionValue});\n");
                }
                else
                {
                    sb.AppendLine(
                        $"{name} = {(isEnum ? $"({fieldType})" : "")}packer.Read{pair.Key}({(hasSize ? $"{pair.Value}" : "")});\n");
                }
            }

            return sb.ToString();
        }
    }
}