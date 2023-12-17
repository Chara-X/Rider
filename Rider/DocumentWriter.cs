using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Rider;

public class DocumentWriter : DocumentComponentBase
{
    public int Position;
    public int RowNumber => Position < Text.Length ? SourceText.Lines.GetLinePosition(Position).Line : 0;
    public int ColumnNumber => Position < Text.Length ? SourceText.Lines.GetLinePosition(Position).Character : 0;

    public async Task SetDocumentAsync(Document document)
    {
        Document = document;
        await OnParametersSetAsync();
    }

    public async Task PrependAsync(string text) => await SetDocumentAsync(Document.WithText(SourceText.Replace(new TextSpan((Position += text.Length) - text.Length, 0), text)));
    public async Task RecedeAsync(int count) => await SetDocumentAsync(Document.WithText(SourceText.Replace(new TextSpan(Position -= count, count), string.Empty)));

    public async Task ReplaceAsync(SyntaxNode oldSyntaxNode, SyntaxNode newSyntaxNode)
    {
        await SetDocumentAsync(Document.WithSyntaxRoot(SyntaxRoot.ReplaceNode(oldSyntaxNode, newSyntaxNode)));
        if (Position >= oldSyntaxNode.Span.End) Position += newSyntaxNode.Span.Length - oldSyntaxNode.Span.Length;
        else if (Position >= oldSyntaxNode.Span.Start) Position += newSyntaxNode.GetText().GetTextChanges(oldSyntaxNode.GetText()).Aggregate(0, (current, x) => current + x.NewText!.Length - x.Span.Length);
    }

    public async Task ReplaceAsync(SyntaxToken oldSyntaxToken, SyntaxToken newSyntaxToken)
    {
        await SetDocumentAsync(Document.WithSyntaxRoot(SyntaxRoot.ReplaceToken(oldSyntaxToken, newSyntaxToken)));
        if (Position >= oldSyntaxToken.Span.End) Position += newSyntaxToken.Span.Length - oldSyntaxToken.Span.Length;
        else if (Position >= oldSyntaxToken.Span.Start) Position = oldSyntaxToken.SpanStart + newSyntaxToken.Span.Length;
    }
}