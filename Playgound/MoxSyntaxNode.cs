using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Playgound;

public class MoxSyntaxNode
{
    protected SyntaxNode ReplaceCore<TNode>(IEnumerable<TNode>? nodes = null, Func<TNode, TNode, SyntaxNode>? computeReplacementNode = null,
        IEnumerable<SyntaxToken>? tokens = null, Func<SyntaxToken, SyntaxToken, SyntaxToken>? computeReplacementToken = null, IEnumerable<SyntaxTrivia>? trivia = null,
        Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia>? computeReplacementTrivia = null)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode ReplaceNodeInListCore(SyntaxNode originalNode, IEnumerable<SyntaxNode> replacementNodes)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode InsertNodesInListCore(SyntaxNode nodeInList, IEnumerable<SyntaxNode> nodesToInsert, bool insertBefore)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode ReplaceTokenInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode InsertTokensInListCore(SyntaxToken originalToken, IEnumerable<SyntaxToken> newTokens, bool insertBefore)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode ReplaceTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode InsertTriviaInListCore(SyntaxTrivia originalTrivia, IEnumerable<SyntaxTrivia> newTrivia, bool insertBefore)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode? RemoveNodesCore(IEnumerable<SyntaxNode> nodes, SyntaxRemoveOptions options)
    {
        throw new NotImplementedException();
    }

    protected SyntaxNode NormalizeWhitespaceCore(string indentation, string eol, bool elasticTrivia)
    {
        throw new NotImplementedException();
    }

    protected bool IsEquivalentToCore(SyntaxNode node, bool topLevel = false)
    {
        throw new NotImplementedException();
    }

    public string Language { get; } = "MOX";
    protected SyntaxTree SyntaxTreeCore { get; }
}