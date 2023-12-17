using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Resharper.Extensions;
using Resharper;
using Rider.Constants;
using Rider.Shared;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;
using Rider.Extensions;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Rider.Components;


public partial class DocumentEditor
{
    private bool _isEditing = true;
    private CodeActor _codeActor = null!;
    private readonly Regex _identifierRegex = new(@"\b\w+$", RegexOptions.RightToLeft);
    private readonly Regex _memberRegex = new(@"\.\w*$", RegexOptions.RightToLeft);
    private ElementReference _codeReference;
    private ElementReference _anchorReference;
    [Inject] private IJSRuntime Js { get; set; } = default!;

    // ReSharper disable ConvertIfStatementToSwitchStatement
    private async Task OnKeyDown(KeyboardEventArgs args)
    {
        try
        {
            _isEditing = !args.CtrlKey || !_isEditing;
            if (args.AltKey && args.IsEnter()) Dialog.Open(RenderCodeActions(), (await Js.GetClientXAsync(_anchorReference), await Js.GetClientYAsync(_anchorReference)));
            else if (args.AltKey && args.IsArrowRight()) Dialog.Open(RenderAutoCompletions(true), (await Js.GetClientXAsync(_anchorReference), await Js.GetClientYAsync(_anchorReference)));
            else if (Dialog.IsOpen && args.IsEnter()) await _codeActor.InvokeAsync();
            else if (Dialog.IsOpen && args.IsArrowUp()) await _codeActor.ArrowUpAsync();
            else if (Dialog.IsOpen && args.IsArrowDown()) await _codeActor.ArrowDownAsync();
            else if (args.IsArrowLeft()) this.ArrowLeft(args.CtrlKey);
            else if (args.IsArrowRight()) this.ArrowRight(args.CtrlKey);
            else if (args.IsArrowUp()) this.ArrowUp();
            else if (args.IsArrowDown()) this.ArrowDown();
            else if (args.IsEnter()) await PrependAsync("\n" + (SyntaxRoot.TryFindNode(Position, SpanType.BraceSpan, out var syntaxNode) ? syntaxNode.GetIndention().Insert(0, "    ") : string.Empty));
            else if (args.Key is "}" or ";")
            {
                await PrependAsync(args.Key);
                await this.ReplaceAsync(SyntaxRoot.FindNode(Position - 1, SpanType.Span)!, x => x.NormalizeWhitespace(eol: "\n").WithIndention(x.GetIndention()).WithTriviaFrom(x));
            }
            else if (args.IsBackspace()) await this.BackspaceAsync();
            else if (args.IsCharacter()) await PrependAsync(args.Key);

            if (Dialog.IsOpen && (args.IsCharacter() || args.IsBackspace())) Dialog.Open(RenderAutoCompletions(false), Dialog.Position);
            else if (!(args.AltKey && args.IsArrowRight()) && !args.IsEnter() && !args.IsArrowUp() && !args.IsArrowDown() && !args.IsCapsLock()) Dialog.Close();
        }
        finally
        {
            await Js.SetSelectionStartAsync(_codeReference, Position);
            await Js.SetSelectionEndAsync(_codeReference, Position);
        }
    }
    // ReSharper restore ConvertIfStatementToSwitchStatement

    private async Task OnClick() => Position = await Js.GetSelectionEndAsync(_codeReference);

    private IEnumerable<(string Name, Func<Task> Action)> GetCodeActions()
    {
        yield return ("Hello, World", async () => await PrependAsync("Hello, World"));
        var syntaxNode = SyntaxRoot.FindNode(Position, SpanType.Span);
        var syntaxToken = SyntaxRoot.FindToken(Position, SpanType.Span);
        if (syntaxToken.IsKind(SyntaxKind.IdentifierToken) && syntaxToken.Parent is ClassDeclarationSyntax classDeclarationSyntax)
            yield return ("Generate Constructor", async () => await SetDocumentAsync(Document.WithSyntaxRoot(SyntaxRoot.ReplaceNode(classDeclarationSyntax, Generations.Constructor(classDeclarationSyntax)))));
        if (syntaxToken.IsKind(SyntaxKind.IdentifierToken) && syntaxToken.Parent != null && SemanticModel.GetSymbol(syntaxToken.Parent) != null)
            yield return ("Rename", async () =>
                    {
                        async Task OnConfirm(string name)
                        {
                            Dialog.Close();
                            Workspace.TryApplyChanges(await Workspace.CurrentSolution.RenameAsync(Document.Id, Position = syntaxToken.SpanStart, name));
                            await SetDocumentAsync(Workspace.CurrentSolution.GetDocument(Document.Id)!);
                            await _codeReference.FocusAsync();
                            await Js.SetSelectionStartAsync(_codeReference, Position);
                            await Js.SetSelectionEndAsync(_codeReference, Position);
                        }

                        Dialog.Open(RenderRename(OnConfirm));
                    }
                );

        if (syntaxNode is not IdentifierNameSyntax identifierNameSyntax) yield break;
        if (SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol is INamedTypeSymbol) yield break;
        var namedTypeSymbols = Compilation.Assembly.GetNamedTypeSymbols().Where(x => x.Name == identifierNameSyntax.Identifier.Text);
        var usingDirectiveSyntaxes = namedTypeSymbols.Select(x => x.ContainingNamespace).Select(x => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(" " + SymbolDisplay.ToDisplayString(x))).WithTrailingTrivia(SyntaxFactory.EndOfLine("\n")));
        foreach (var x in usingDirectiveSyntaxes) yield return ($"Using '{x.Name}'", async () => await ReplaceAsync(SyntaxRoot, SyntaxRoot.AddUsings(x)));
    }

    private IEnumerable<(string Name, Func<Task> Action)> GetAutoCompletions()
    {
        IEnumerable<string> names = Array.Empty<string>();
        var prefix = Text[..Position];
        Capture capture;
        // 性能问题：LookupSymbols，GetSymbol
        if ((capture = _memberRegex.Match(prefix)).Length > 0)
        {
            var member = capture.Value[1..];
            var namespaceOrTypeSymbol = SemanticModel.GetSymbol(SyntaxRoot.FindNode(Position - capture.Length - 1, SpanType.Span)!) switch { ILocalSymbol s => s.Type, INamespaceOrTypeSymbol s => s, _ => null };
            if (namespaceOrTypeSymbol != null) names = namespaceOrTypeSymbol.GetMembers().Where(x => x.DeclaredAccessibility == Accessibility.Public && x is not IMethodSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary }).Select(x => x.Name).Distinct().Where(x => x.StartsWith(member));
        }
        else if ((capture = _identifierRegex.Match(prefix)).Length >= 0) names = SyntaxConstants.Keywords.Concat(SemanticModel.LookupSymbols(Position).Select(x => x.Name)).Distinct().Where(x => x.StartsWith(capture.Value));

        return names.Select(x => ValueTuple.Create(x, async () => await PrependAsync(x[_identifierRegex.Match(Text[..Position]).Length..])));
    }
}