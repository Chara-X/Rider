using Microsoft.AspNetCore.Components;

namespace Rider.Components;

public partial class Dialog
{
    private static RenderFragment? _childContent;
    private static (double ClientX, double ClientY)? _position;
    public static bool IsOpen => _childContent is not null;
    public static (double ClientX, double ClientY)? Position => _position;
    public static event Action? OnOpen;
    public static event Action? OnClose;
    public Dialog() => (OnOpen, OnClose) = (StateHasChanged, StateHasChanged);

    public static void Open(RenderFragment childContent, (double ClientX, double ClientY)? position = null)
    {
        (_childContent, _position) = (childContent, position);
        OnOpen?.Invoke();
    }

    public static void Close()
    {
        (_childContent, _position) = (null, null);
        OnClose?.Invoke();
    }
}