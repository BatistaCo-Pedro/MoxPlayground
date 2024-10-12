using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Playgound.SyntaxConstructs;

public partial struct SyntaxHelper
{
     public static AssignmentExpressionSyntax CoalesceAssignment(ExpressionSyntax target, ExpressionSyntax source)
    {
        return AssignmentExpression(
            SyntaxKind.CoalesceAssignmentExpression,
            target,
            SpacedToken(SyntaxKind.QuestionQuestionEqualsToken),
            source
        );
    }
}