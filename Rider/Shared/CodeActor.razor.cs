using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Rider.Components;
using Rider.Extensions;

namespace Rider.Shared;

public partial class CodeActor
{
    private int _index;
    private ElementReference _actionsReference;
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [Parameter] public (string Name, Func<Task> Action)[] CodeActions { get; set; } = Array.Empty<(string name, Func<Task> action)>();

    protected override void OnParametersSet()
    {
        _index = 0;
        if (CodeActions.Length == 0) Dialog.Close();
    }

    public async Task InvokeAsync()
    {
        Dialog.Close();
        await CodeActions[_index].Action();
    }

    public async Task ArrowUpAsync()
    {
        if (--_index == -1) _index = CodeActions.Length - 1;
        await (await Js.GetChildAsync(_actionsReference, _index)).ScrollIntoViewAsync(true);
        StateHasChanged();
    }

    public async Task ArrowDownAsync()
    {
        if (++_index == CodeActions.Length) _index = 0;
        await (await Js.GetChildAsync(_actionsReference, _index)).ScrollIntoViewAsync(false);
        StateHasChanged();
    }
}