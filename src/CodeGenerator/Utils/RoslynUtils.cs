using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator
{
    internal static class RoslynUtils
    {
        private const string AttributePostfixName = "Attribute";

        internal static void ReplaceAllNodesInCycle<TRootNode, TReplaceNode>(ref TRootNode rootNode,
            TReplaceNode newNode)
            where TRootNode : SyntaxNode
            where TReplaceNode : SyntaxNode
        {
            var i = 0;
            while (true)
            {
                var oldNode = rootNode
                    .DescendantNodes()
                    .OfType<TReplaceNode>()
                    .ElementAtOrDefault(i);

                if (oldNode == null)
                    break;

                i++;
                rootNode = rootNode.ReplaceNode(oldNode, newNode);
            }
        }

        internal static CompilationUnitSyntax RemoveAllNodesInRootByType<TNode>(this CompilationUnitSyntax root)
            where TNode : SyntaxNode
        {
            var newRoot = root;
            var i = 0;
            while (true)
            {
                var oldNode = root
                    .DescendantNodes()
                    .OfType<TNode>()
                    .ElementAtOrDefault(i);

                if (oldNode == null)
                    break;

                i++;
                root = root.RemoveNode(oldNode, SyntaxRemoveOptions.KeepNoTrivia);
                newRoot = root;
            }

            return newRoot;
        }
        
        internal static void RemoveAllNodesInCycle<TRootNode, TReplaceNode>(ref TRootNode rootNode)
            where TRootNode : SyntaxNode
            where TReplaceNode : SyntaxNode
        {
            var i = 0;
            while (true)
            {
                var oldNode = rootNode
                    .DescendantNodes()
                    .OfType<TReplaceNode>()
                    .ElementAtOrDefault(i);

                if (oldNode == null)
                    break;

                i++;
                rootNode = rootNode.RemoveNode(oldNode, SyntaxRemoveOptions.KeepNoTrivia);
            }
        }

        internal static bool CanRemoveNodeWithAttribute(FieldDeclarationSyntax field,
            ICollection<string> attributeNames)
        {
            return field.AttributeLists.Count > 0 && field.AttributeLists
                .Select(fieldAttributeList =>
                    fieldAttributeList.Attributes.Any(x => AttributeNameMatches(x, attributeNames)))
                .Any(canRemoveNode => canRemoveNode);
        }

        internal static SyntaxList<AttributeListSyntax> GetRemovableAttributes(TypeDeclarationSyntax node,
            ICollection<string> attributeNames)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            return (from attributeList in node.AttributeLists
                let nodesToRemove = attributeList.Attributes
                    .Where(attribute => AttributeNameMatches(attribute, attributeNames))
                    .ToArray()
                select attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepEndOfLine)
                into newAttribute
                where newAttribute.Attributes.Count > 0
                select newAttribute).Aggregate(newAttributes, (current, newAttribute) => current.Add(newAttribute));
        }

        internal static SyntaxList<AttributeListSyntax> GetRemovableAttributes(MemberDeclarationSyntax node,
            ICollection<string> attributeNames)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            return (from attributeList in node.AttributeLists
                let nodesToRemove = attributeList.Attributes
                    .Where(attribute => AttributeNameMatches(attribute, attributeNames))
                    .ToArray()
                select attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia)
                into newAttribute
                where newAttribute.Attributes.Count > 0
                select newAttribute).Aggregate(newAttributes, (current, newAttribute) => current.Add(newAttribute));
        }

        private static SimpleNameSyntax GetSimpleNameFromNode(AttributeSyntax node)
        {
            var identifierNameSyntax = node.Name as IdentifierNameSyntax;
            var qualifiedNameSyntax = node.Name as QualifiedNameSyntax;

            return
                identifierNameSyntax
                ??
                qualifiedNameSyntax?.Right
                ??
                (node.Name as AliasQualifiedNameSyntax).Name;
        }

        private static bool AttributeNameMatches(AttributeSyntax attribute, ICollection<string> attributeNames)
        {
            return
                attributeNames.Contains(GetSimpleNameFromNode(attribute)
                    .Identifier
                    .Text
                    .RemovePartOfString(AttributePostfixName));
        }
    }
}