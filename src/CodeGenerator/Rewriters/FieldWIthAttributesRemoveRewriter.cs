using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.Rewriters
{
    public class FieldWIthAttributesRemoveRewriter : CSharpSyntaxRewriter
    {
        private readonly HashSet<string> _attributeNames;
        private readonly string _attributePostfixName = "Attribute";

        public FieldWIthAttributesRemoveRewriter(params string[] attributeNames)
        {
            _attributeNames = new HashSet<string>();
            foreach (var attributeName in attributeNames)
            {
                _attributeNames.Add(attributeName.RemovePartOfString(_attributePostfixName));
            }
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var fields = node.Members.OfType<FieldDeclarationSyntax>();
            var removableListFields = fields.Where(field => RoslynUtils.CanRemoveNodeWithAttribute(field, _attributeNames)).ToList();
            node = node.RemoveNodes(removableListFields, SyntaxRemoveOptions.KeepTrailingTrivia |
                                                         SyntaxRemoveOptions.KeepLeadingTrivia);

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var fields = node.Members.OfType<FieldDeclarationSyntax>();
            var removableListFields = fields.Where(field => RoslynUtils.CanRemoveNodeWithAttribute(field, _attributeNames)).ToList();
            node = node.RemoveNodes(removableListFields, SyntaxRemoveOptions.KeepTrailingTrivia |
                                                         SyntaxRemoveOptions.KeepLeadingTrivia);
            return base.VisitStructDeclaration(node);
        }
    }
}