using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Playgound.SyntaxConstructs;

public partial struct SyntaxHelper
{
    public NamespaceDeclarationSyntax Namespace(string namespaceName)
    {
        return NamespaceDeclaration(IdentifierName(namespaceName))
            .WithNamespaceKeyword(TrailingSpacedToken(SyntaxKind.NamespaceKeyword))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken));
    }
    
    public ClassDeclarationSyntax Class(string className, SyntaxTokenList modifiers, SyntaxList<MemberDeclarationSyntax> members)
    {
        return ClassDeclaration(Identifier(className))
            .WithModifiers(modifiers)
            .WithMembers(members)
            .WithKeyword(TrailingSpacedToken(SyntaxKind.ClassKeyword))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken))
            .AddLeadingLineFeed(Indentation);
    }

    public TypeDeclarationSyntax TypeDeclaration(TypeDeclarationSyntax syntax,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        var name = syntax.Identifier;
        TypeDeclarationSyntax type = syntax switch
        {
            ClassDeclarationSyntax => ClassDeclaration(name),
            StructDeclarationSyntax => StructDeclaration(name),
            InterfaceDeclarationSyntax => InterfaceDeclaration(name),
            RecordDeclarationSyntax => RecordDeclaration(Token(SyntaxKind.RecordKeyword), name),
            _ => throw new NotSupportedException($"Unsupported type declaration syntax {syntax.GetType().Name}."),
        };
        
        return type.WithModifiers(syntax.Modifiers)
            .WithMembers(members)
            .WithoutTrivia()
            .WithKeyword(TrailingSpacedToken(syntax.Keyword.Kind()))
            .WithOpenBraceToken(LeadingLineFeedToken(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(LeadingLineFeedToken(SyntaxKind.CloseBraceToken))
            .AddLeadingLineFeed(Indentation);
    }
}