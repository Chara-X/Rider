using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using System.IO.Compression;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

// ReSharper disable TailRecursiveCall

namespace Rider.Components;

public partial class WorkspaceExplorer
{
    private readonly ExplorerDirectory _workDirectory = new() { Name = nameof(Rider) };
    private ExplorerFile? _currentFile;
    [Inject] public IJSRuntime Js { get; set; } = null!;
    [Inject] public HttpClient Http { get; set; } = null!;
    [CascadingParameter] private Workspace Workspace { get; set; } = null!;
    [Parameter] public EventCallback<Document> ActiveDocumentChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), nameof(Rider), nameof(Rider), LanguageNames.CSharp);
        Workspace.TryApplyChanges(Workspace.CurrentSolution.AddProject(projectInfo));
        using var zipArchive = new ZipArchive(await Http.GetStreamAsync("src.zip"));
        var solution = Workspace.CurrentSolution;
        foreach (var x in zipArchive.Entries)
        {
            if (x.ExternalAttributes != 32) continue;
            using var reader = new StreamReader(x.Open());
            solution = solution.AddDocument(DocumentId.CreateNewId(projectInfo.Id), x.Name, (await reader.ReadToEndAsync()).Replace("\r\n", "\n"), filePath: Path.GetRelativePath("src", x.FullName));
        }

        var project = solution.Projects.Single();
        project = project.AddMetadataReference(MetadataReference.CreateFromStream(await Http.GetStreamAsync("libraries/System.Private.CoreLib.dll")));
        project = project.AddMetadataReference(MetadataReference.CreateFromStream(await Http.GetStreamAsync("libraries/System.Runtime.dll")));
        project = project.AddMetadataReference(MetadataReference.CreateFromStream(await Http.GetStreamAsync("libraries/System.Console.dll")));
        Workspace.TryApplyChanges(project.Solution);
        Workspace.CurrentSolution.Projects.Single().Documents.ToList().ForEach(x => AddFile(GetDirectory(x.FilePath), x.Name, x.Id));
    }

    private async Task OnExplorerFileClick(ExplorerFile file) => await ActiveDocumentChanged.InvokeAsync(Workspace.CurrentSolution.GetDocument((_currentFile = file).DocumentId)!);
    private void OnContextMenu(ExplorerNode explorerNode, MouseEventArgs args) => Dialog.Open(RenderCodeActions(explorerNode), (args.ClientX, args.ClientY));

    private ExplorerDirectory GetDirectory(string? filePath) => GetDirectory(_workDirectory, new Queue<string>((Path.GetDirectoryName(filePath) ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)));

    private static ExplorerDirectory GetDirectory(ExplorerDirectory directory, Queue<string> directoryNames)
    {
        if (directoryNames.Count == 0) return directory;
        var directoryName = directoryNames.Dequeue();
        if (directory.Children.All(x => x.Name != directoryName)) AddDirectory(directory, directoryName);
        return GetDirectory((ExplorerDirectory)directory.Children.Single(x => x.Name == directoryName), directoryNames);
    }

    private static void AddDirectory(ExplorerDirectory directory, string name) => directory.Add(new ExplorerDirectory { Name = name });
    private static void AddFile(ExplorerDirectory directory, string name, DocumentId documentId) => directory.Add(new ExplorerFile { Name = name, DocumentId = documentId });
    private void New<T>(ExplorerDirectory directory) where T : ExplorerNode
    {
        var explorerNode = Activator.CreateInstance<T>();
        Dialog.Open(RenderRename(OnConfirm));
        return;

        async Task OnConfirm(string name)
        {
            explorerNode.Name = name;
            directory.Add(explorerNode);
            Dialog.Close();
            if (explorerNode is not ExplorerFile file) return;
            var document = Workspace.CurrentSolution.Projects.Single().AddDocument(explorerNode.Name, string.Empty);
            file.DocumentId = document.Id;
            Workspace.TryApplyChanges(document.Project.Solution);
            await OnExplorerFileClick(file);
        }
    }

    private void Dispose(ExplorerNode explorerNode)
    {
        explorerNode.Parent.Children.Remove(explorerNode);
        switch (explorerNode)
        {
            case ExplorerDirectory directory:
            {
                foreach (var x in Workspace.CurrentSolution.Projects.Single().Documents.Where(x => x.FilePath!.StartsWith(directory.Name)))
                    Workspace.TryApplyChanges(Workspace.CurrentSolution.RemoveDocument(x.Id));
                break;
            }
            case ExplorerFile file:
                Workspace.TryApplyChanges(Workspace.CurrentSolution.RemoveDocument(file.DocumentId));
                break;
        }

        _currentFile = _currentFile == explorerNode ? null : _currentFile;
        StateHasChanged();
    }

    private IEnumerable<(string Name, Func<Task> Action)> GetCodeActions(ExplorerNode explorerNode)
    {
        if (explorerNode is ExplorerDirectory directory)
        {
            yield return ("New Directory", async () => New<ExplorerDirectory>(directory));
            yield return ("New File", async () => New<ExplorerFile>(directory));
        }

        yield return ("Dispose", async () => Dispose(explorerNode));
    }

    private class ExplorerNode
    {
        public string Name { get; set; } = null!;
        public ExplorerDirectory Parent { get; set; } = null!;
    }

    private class ExplorerDirectory : ExplorerNode
    {
        public SortedSet<ExplorerNode> Children { get; init; } = new(new ExplorerComparer());

        public void Add(ExplorerNode explorerNode)
        {
            explorerNode.Parent = this;
            Children.Add(explorerNode);
        }
    }

    private class ExplorerFile : ExplorerNode
    {
        public DocumentId DocumentId { get; set; } = null!;
    }

    private class ExplorerComparer : IComparer<ExplorerNode>
    {
        public int Compare(ExplorerNode x, ExplorerNode y) => x switch { ExplorerDirectory when y is ExplorerFile => -1, ExplorerFile when y is ExplorerDirectory => 1, _ => string.Compare(x.Name, y.Name, StringComparison.Ordinal) };
    }
}