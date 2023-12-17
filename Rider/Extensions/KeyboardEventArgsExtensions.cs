using Microsoft.AspNetCore.Components.Web;

namespace Rider.Extensions;

public static class KeyboardEventArgsExtensions
{
    public static bool IsBackspace(this KeyboardEventArgs args) => args.Key == "Backspace";
    public static bool IsCapsLock(this KeyboardEventArgs args) => args.Key == "CapsLock";
    public static bool IsArrowLeft(this KeyboardEventArgs args) => args.Key == "ArrowLeft";
    public static bool IsArrowRight(this KeyboardEventArgs args) => args.Key == "ArrowRight";
    public static bool IsArrowUp(this KeyboardEventArgs args) => args.Key == "ArrowUp";
    public static bool IsArrowDown(this KeyboardEventArgs args) => args.Key == "ArrowDown";
    public static bool IsEnter(this KeyboardEventArgs args) => args.Key == "Enter";
    public static bool IsCharacter(this KeyboardEventArgs args) => args.Key.Length == 1;
}