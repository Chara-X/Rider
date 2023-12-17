using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Rider.Extensions;

namespace Rider.Shared;

public partial class Rename
{
    private string _name = string.Empty;
    private ElementReference _inputReference;
    [Parameter] public EventCallback<string> OnChanged { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await _inputReference.FocusAsync();
    }

    private void OnKeyPress(KeyboardEventArgs args)
    {
        if (args.IsEnter()) OnChanged.InvokeAsync(_name);
    }
}