using Microsoft.JSInterop;

namespace Rider.Extensions;

public static class JsObjectReferenceExtensions
{
    public static async Task ScrollIntoViewAsync(this IJSObjectReference jsObjectReference, bool alignToTop) => await jsObjectReference.InvokeVoidAsync("scrollIntoView", alignToTop);
}