using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeGenerator.Rewriters
{
    public class RemoveCommentsRewriter : CSharpSyntaxRewriter
    {
        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                return default;
            
            return base.VisitTrivia(trivia);
        }
    }
}