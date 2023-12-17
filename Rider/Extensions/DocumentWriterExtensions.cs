using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rider.Extensions;

public static class DocumentWriterExtensions
{
    public static async Task ReplaceAsync<T>(this DocumentWriter writer, T oldSyntaxNode, Func<T, SyntaxNode> computeReplacementNode) where T : SyntaxNode => await writer.ReplaceAsync(oldSyntaxNode, computeReplacementNode(oldSyntaxNode));
    public static async Task ReplaceAsync(this DocumentWriter writer, SyntaxToken oldSyntaxToken, Func<SyntaxToken, SyntaxToken> computeReplacementToken) => await writer.ReplaceAsync(oldSyntaxToken, computeReplacementToken(oldSyntaxToken));

    public static void ArrowLeft(this DocumentWriter writer, bool ctrlKey)
    {
        if (writer.Position == 0) return;
        writer.Position -= ctrlKey ? writer.Position - writer.SyntaxRoot.FindToken(writer.Position).GetPreviousToken().SpanStart : 1;
    }

    public static void ArrowRight(this DocumentWriter writer, bool ctrlKey)
    {
        if (writer.Position == writer.SourceText.Length) return;
        writer.Position += ctrlKey ? writer.SyntaxRoot.FindToken(writer.Position).GetNextToken().Span.End - writer.Position : 1;
    }

    public static void ArrowUp(this DocumentWriter writer)
    {
        if (writer.RowNumber == 0) return;
        writer.Position -= (writer.SourceText.Lines[writer.RowNumber - 1].Span.Length >= writer.ColumnNumber ? writer.SourceText.Lines[writer.RowNumber - 1].Span.Length + 1 : writer.ColumnNumber + 1);
    }

    public static void ArrowDown(this DocumentWriter writer)
    {
        if (writer.RowNumber == writer.RowsCount - 1) return;
        writer.Position += writer.SourceText.Lines[writer.RowNumber + 1].Span.Length >= writer.ColumnNumber ? writer.SourceText.Lines[writer.RowNumber].Span.Length + 1 : writer.SourceText.Lines[writer.RowNumber].Span.Length - (writer.ColumnNumber - writer.SourceText.Lines[writer.RowNumber + 1].Span.Length) + 1;
    }

    public static async Task BackspaceAsync(this DocumentWriter writer)
    {
        var syntaxTrivia = writer.SyntaxRoot.FindTrivia(writer.Position - 1);
        if (syntaxTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
            await writer.RecedeAsync(writer.Position - syntaxTrivia.SpanStart);
        else await writer.RecedeAsync(1);
    }
}