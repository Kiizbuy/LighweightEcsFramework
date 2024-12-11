using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.Rewriters
{
    public class RemoveAllAttributesFromNodeRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            foreach (var attributeList in node.AttributeLists)
            {
                var nodesToRemove =
                    attributeList
                        .Attributes;

                if (nodesToRemove.Any() && nodesToRemove.Count != attributeList.Attributes.Count)
                {
                    var newAttribute = 
                        (AttributeListSyntax)VisitAttributeList(
                            attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));
						
                    newAttributes = newAttributes.Add(newAttribute);
                }			
            }
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            //Append the leading trivia to the method
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            foreach (var attributeList in node.AttributeLists)
            {
                var nodesToRemove =
                    attributeList
                        .Attributes;

                if (nodesToRemove.Any() && nodesToRemove.Count != attributeList.Attributes.Count)
                {
                    var newAttribute = 
                        (AttributeListSyntax)VisitAttributeList(
                            attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));
						
                    newAttributes = newAttributes.Add(newAttribute);
                }			
            }
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            //Append the leading trivia to the method
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            foreach (var attributeList in node.AttributeLists)
            {
                var nodesToRemove =
                    attributeList
                        .Attributes;

                if (nodesToRemove.Any() && nodesToRemove.Count != attributeList.Attributes.Count)
                {
                    var newAttribute = 
                        (AttributeListSyntax)VisitAttributeList(
                            attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));
						
                    newAttributes = newAttributes.Add(newAttribute);
                }			
            }
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            //Append the leading trivia to the method
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }

        public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var newAttributes = new SyntaxList<AttributeListSyntax>();

            foreach (var attributeList in node.AttributeLists)
            {
                var nodesToRemove =
                    attributeList
                        .Attributes;

                if (nodesToRemove.Any() && nodesToRemove.Count != attributeList.Attributes.Count)
                {
                    var newAttribute = 
                        (AttributeListSyntax)VisitAttributeList(
                            attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));
						
                    newAttributes = newAttributes.Add(newAttribute);
                }			
            }
            var leadTriv = node.GetLeadingTrivia();
            node = node.WithAttributeLists(newAttributes);

            //Append the leading trivia to the method
            node = node.WithLeadingTrivia(leadTriv);
            return node;
        }
    }
}