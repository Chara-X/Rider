using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;

namespace Rider.Components;

public partial class Shell
{
    private readonly StringWriter _stringWriter = new();
    [CascadingParameter] public Workspace Workspace { get; set; } = null!;

    private async Task OnRunClick()
    {
        var memoryStream = new MemoryStream();
        (await Workspace.CurrentSolution.Projects.Single().GetCompilationAsync())!.Emit(memoryStream);
        _stringWriter.GetStringBuilder().Clear();
        Console.SetOut(_stringWriter);
        Assembly.Load(memoryStream.ToArray()).GetTypes().Single(x => x.Name == "Program").GetMethods().Single(x => x.Name == "Main").Invoke(null, Array.Empty<object?>());
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    }
}