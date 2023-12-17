using Microsoft.AspNetCore.Components;

namespace Rider.Extensions;

public static class ChangeEventArgsExtensions
{
    public static string ValueOrEmpty(this ChangeEventArgs args) => args.Value?.ToString() ?? string.Empty;
}