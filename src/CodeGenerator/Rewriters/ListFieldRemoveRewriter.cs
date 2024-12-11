using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.Rewriters
{
    public class ListFieldRemoveRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var fields = node.Members.OfType<FieldDeclarationSyntax>();
            var removableListFields =
                (from field in fields
                    let generic = field.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault()
                    where generic != null && generic.IsList()
                    select field).ToList();

            node = node.RemoveNodes(removableListFields, SyntaxRemoveOptions.KeepTrailingTrivia |
                                                         SyntaxRemoveOptions.KeepLeadingTrivia);

            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var fields = node.Members.OfType<FieldDeclarationSyntax>();
            var removableListFields =
                (from field in fields
                    let generic = field.DescendantNodes().OfType<GenericNameSyntax>().FirstOrDefault()
                    where generic != null && generic.IsList()
                    select field).ToList();

            node = node.RemoveNodes(removableListFields, SyntaxRemoveOptions.KeepTrailingTrivia |
                                                         SyntaxRemoveOptions.KeepLeadingTrivia);

            return base.VisitStructDeclaration(node);
        }
    }
}