using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace Rider;

public class DocumentComponentBase : ComponentBase
{
    public Workspace Workspace => Document.Project.Solution.Workspace;
    public Compilation Compilation { get; private set; } = null!;
    public SemanticModel SemanticModel { get; private set; } = null!;
    public SyntaxTree SyntaxTree { get; private set; } = null!;
    public CompilationUnitSyntax SyntaxRoot { get; private set; } = null!;
    public SourceText SourceText { get; private set; } = null!;
    public string Text { get; private set; } = null!;
    public int RowsCount { get; private set; }
    public int ColumnsCount { get; private set; }
    [Parameter] public Document Document { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        Compilation = (await Document.Project.GetCompilationAsync())!;
        SemanticModel = (await Document.GetSemanticModelAsync())!;
        SyntaxTree = (await Document.GetSyntaxTreeAsync())!;
        SyntaxRoot = (CompilationUnitSyntax)(await Document.GetSyntaxRootAsync())!;
        SourceText = await Document.GetTextAsync();
        Text = SourceText.ToString();
        RowsCount = SourceText.Lines.Count;
        ColumnsCount = SourceText.Lines.Max(x => x.Span.Length);
    }
}