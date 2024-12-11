using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.Rewriters
{
    public class AttributeRemoveRewriter : CSharpSyntaxRewriter
    {
        private readonly HashSet<string> _attributeNames;
        private readonly string _attributePostfixName = "Attribute";
        
        public AttributeRemoveRewriter(params string[] attributeNames)
        {
            _attributeNames = new HashSet<string>();
            foreach (var name in attributeNames)
            {
                _attributeNames.Add(name.RemovePartOfString(_attributePostfixName));
            }
        }


        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            if (node.AttributeLists.Count == 0)
            {
                return base.VisitStructDeclaration(node);
            }
            
            ReplaceFieldsInCycle(ref node);
            
            var newAttributes = RoslynUtils.GetRemovableAttributes(node, _attributeNames);
            var leadTriv = node.GetLeadingTrivia();
          
            node = node.WithAttributeLists(newAttributes);
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.AttributeLists.Count == 0)
            {
                return base.VisitClassDeclaration(node);
            }

            ReplaceFieldsInCycle(ref node);

            var newAttributes = RoslynUtils.GetRemovableAttributes(node, _attributeNames);
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.AttributeLists.Count == 0)
            {
                return base.VisitFieldDeclaration(node);
            }
            
            var newAttributes = RoslynUtils.GetRemovableAttributes(node, _attributeNames);
            var leadTriv = node.GetLeadingTrivia();
            
            node = node.WithAttributeLists(newAttributes);
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.AttributeLists.Count == 0)
            {
                return base.VisitMethodDeclaration(node);
            }
            
            var newAttributes = RoslynUtils.GetRemovableAttributes(node, _attributeNames);
            var leadTriv = node.GetLeadingTrivia();
            
            node = node.WithAttributeLists(newAttributes);
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        //Cancer, but in order to replace a node in a cycle, you need check old fields values, because immutable statement :)
        private void ReplaceFieldsInCycle<T>(ref T typedNode) where T: SyntaxNode
        {
            var i = 0;
            while (true)
            {
                var fieldDeclaration = typedNode
                    .DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .ElementAtOrDefault(i);

                if (fieldDeclaration == null)
                    break;

                i++;
                typedNode = typedNode.ReplaceNode(fieldDeclaration, VisitFieldDeclaration(fieldDeclaration));
            }
        }
        
       
    }
}