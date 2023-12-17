using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Rider.Extensions;

public static class JsRuntimeExtensions
{
    public static async Task<double> GetScrollTopAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<double>("GetScrollTopAsync", elementReference);
    public static async Task SetScrollTopAsync(this IJSRuntime jsRuntime, ElementReference elementReference, double scrollTop) => await jsRuntime.InvokeVoidAsync("SetScrollTopAsync", elementReference, scrollTop);
    public static async Task<IJSObjectReference> GetChildAsync(this IJSRuntime jsRuntime, ElementReference elementReference, int index) => await jsRuntime.InvokeAsync<IJSObjectReference>("GetChildAsync", elementReference, index);
    public static async Task SetChildAsync(this IJSRuntime jsRuntime, ElementReference elementReference, int index, ElementReference child) => await jsRuntime.InvokeVoidAsync("SetChildAsync", elementReference, index, child);
    public static async Task<int> GetSelectionStartAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<int>("GetSelectionStartAsync", elementReference);
    public static async Task SetSelectionStartAsync(this IJSRuntime jsRuntime, ElementReference elementReference, int selectionStart) => await jsRuntime.InvokeVoidAsync("SetSelectionStartAsync", elementReference, selectionStart);
    public static async Task<int> GetSelectionEndAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<int>("GetSelectionEndAsync", elementReference);
    public static async Task SetSelectionEndAsync(this IJSRuntime jsRuntime, ElementReference elementReference, int selectionEnd) => await jsRuntime.InvokeVoidAsync("SetSelectionEndAsync", elementReference, selectionEnd);
    public static async Task<string> GetValueAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<string>("GetValueAsync", elementReference);
    public static async Task SetValueAsync(this IJSRuntime jsRuntime, ElementReference elementReference, string value) => await jsRuntime.InvokeVoidAsync("SetValueAsync", elementReference, value);
    public static async Task<bool> GetOpenAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<bool>("GetOpenAsync", elementReference);
    public static async Task SetOpenAsync(this IJSRuntime jsRuntime, ElementReference elementReference, bool open) => await jsRuntime.InvokeVoidAsync("SetOpenAsync", elementReference, open);
    public static async Task<double> GetClientXAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<double>("GetClientXAsync", elementReference);
    public static async Task<double> GetClientYAsync(this IJSRuntime jsRuntime, ElementReference elementReference) => await jsRuntime.InvokeAsync<double>("GetClientYAsync", elementReference);
    public static async Task ClipboardCopyAsync(this IJSRuntime jsRuntime, string text) => await jsRuntime.InvokeVoidAsync("ClipboardCopyAsync", text);
    public static async Task<string> ClipboardPasteAsync(this IJSRuntime jsRuntime) => await jsRuntime.InvokeAsync<string>("ClipboardPasteAsync");
}