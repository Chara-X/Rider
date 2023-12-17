using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;

namespace Rider.BlazorApp.Pages;

[Route(nameof(Home))]
public partial class Home
{
    private readonly Workspace _workspace = new AdhocWorkspace();
    private Document? _activeDocument;
}