using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGenerator.DeclarationGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CodeGenerator
{
    public class RoslynCodeGenerator
    {
        internal readonly List<string> ExternalLibs = new List<string>();

        public void AddExternalLibraryPathReferences(IEnumerable<string> libs)
        {
            ExternalLibs.AddRange(libs);
        }

        public void Generate(IEnumerable<string> csFileNames)
        {
            GenerateFixedArrays(csFileNames);
            GenerateComponentResolvers(csFileNames);
        }

        private void GenerateFixedArrays(IEnumerable<string> csFileNames)
        {
            var rootTree = RoslynDeclarationsGenerator.CreateRootSyntaxTree();
            var root = rootTree.GetCompilationUnitRoot();
            var nameSpace = "EcsCore.Data.FixedArrays".ToNamespace();
            var fixedArrays = FetchAllStructsMetaDataFrom(csFileNames, "IFixedArray").ToList();
            root = root.AddUsings("System".ToUsing(),
                "System.Runtime.CompilerServices".ToUsing(),
                "System.Runtime.InteropServices".ToUsing());

            foreach (var fixedArray in fixedArrays)
            {
                var fixedArrayDeclaration = default(FixedArrayDeclarationGenerator);
                var generatedFixedArray =
                    fixedArrayDeclaration.TryGenerateFixedArrayImplementation(fixedArray.declaration,
                        fixedArray.semanticModel
                    );

                if (generatedFixedArray != default)
                    nameSpace = nameSpace.AddMembers(generatedFixedArray);
            }

            root = root.AddMembers(nameSpace);
            Console.WriteLine(root.NormalizeWhitespace().ToString());
        }

        private void GenerateComponentResolvers(IEnumerable<string> csFileNames)
        {
            var rootTree = RoslynDeclarationsGenerator.CreateRootSyntaxTree();
            var root = rootTree.GetCompilationUnitRoot();
            var nameSpace = "EcsCore.Serialization.Resolvers".ToNamespace();
            var components = FetchAllStructsMetaDataFrom(csFileNames, "IComponentData").ToList();
            var resolverMap = RoslynDeclarationsGenerator.GenerateEmptyClassDeclaration("ResolversMap",
                RoslynDeclarationsGenerator.GetModifiers(SyntaxKind.InternalKeyword, SyntaxKind.StaticKeyword,
                    SyntaxKind.PartialKeyword), 
                null);
            var resolverMapInitializeMethod = RoslynDeclarationsGenerator.GenerateEmptyMethodDeclaration(
                RoslynDeclarationsGenerator.GetModifiersList(SyntaxKind.StaticKeyword,
                    SyntaxKind.PartialKeyword),
                "void",
                "Initialize");
            root = root.AddUsings("Components".ToUsing());

            foreach (var component in components)
            {
                var componentResolverDeclarationGenerator = default(ComponentResolversDeclatationGenerator);
                var generatedComponentResolver =
                    componentResolverDeclarationGenerator.GenerateComponentResolverImplementation(component.declaration,
                        component.semanticModel
                    );
                resolverMapInitializeMethod = resolverMapInitializeMethod
                    .AddBodyStatements(
                        $"_componentResolvers.TryAdd(nameof({component.declaration.Identifier.ValueText}), new {generatedComponentResolver.Identifier.ValueText}());"
                            .ToStatement().NormalizeWhitespace());

                nameSpace = nameSpace.AddMembers(generatedComponentResolver);
            }

            resolverMap = resolverMap.AddMembers(resolverMapInitializeMethod);
            nameSpace = nameSpace.AddMembers(resolverMap);

            root = root.AddMembers(nameSpace);

            Console.WriteLine(root.NormalizeWhitespace().ToString());
        }

        private IEnumerable<(StructDeclarationSyntax declaration, SemanticModel semanticModel)>
            FetchAllStructsMetaDataFrom(
                IEnumerable<string> componentFileNames, string inheritor)
        {
            foreach (var file in componentFileNames)
            {
                var tree = file.ParseFileToSyntaxTree();
                var root = tree.GetCompilationUnitRoot();
                var compilation = CSharpCompilation.Create($"{inheritor}Generator")
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .WithReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .WithReferences(ExternalLibs.Select(x => MetadataReference.CreateFromFile(x)))
                    .AddSyntaxTrees(tree);
                var semanticModel = compilation.GetSemanticModel(tree);
                var possibleComponentDeclarationCollection =
                    root.DescendantNodesAndSelf().OfType<StructDeclarationSyntax>();

                foreach (var possibleComponent in possibleComponentDeclarationCollection)
                {
                    if (possibleComponent.InheritsFrom(inheritor))
                    {
                        yield return (possibleComponent, semanticModel);
                    }
                }
            }
        }
    }
}