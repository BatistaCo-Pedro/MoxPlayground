using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Playgound.SyntaxConstructs;

public partial struct SyntaxHelper
{
    private static readonly SyntaxTriviaList SpaceTriviaList = SyntaxTriviaList.Create(ElasticSpace);

    private SyntaxToken LeadingLineFeedToken(SyntaxKind kind)
    {
        return Token(SyntaxTriviaList.Empty.AddLineFeedAndIndentation(Indentation), kind, SyntaxTriviaList.Empty);
    }

    private SyntaxToken TrailingLineFeedToken(SyntaxKind kind, int indentation)
    {
        return Token(SyntaxTriviaList.Empty, kind, SyntaxTriviaList.Empty.AddLineFeedAndIndentation(indentation));
    }

    private SyntaxToken LeadingLineFeedTrailingSpaceToken(SyntaxKind kind)
    {
        return Token(SyntaxTriviaList.Empty.AddLineFeedAndIndentation(Indentation), kind, SpaceTriviaList);
    }

    private static SyntaxToken LeadingSpacedToken(SyntaxKind kind) => Token(SpaceTriviaList, kind, SyntaxTriviaList.Empty);

    public static SyntaxToken TrailingSpacedToken(SyntaxKind kind) => Token(SyntaxTriviaList.Empty, kind, SpaceTriviaList);

    private static SyntaxToken SpacedToken(SyntaxKind kind) => Token(SpaceTriviaList, kind, SpaceTriviaList);
}