using System.Runtime.CompilerServices;

namespace ServerAppDesktop.Controls;

internal static class TrayDelegateHandler
{
    internal static event Action<HWND, uint, WPARAM, LPARAM, nuint, nuint>? Invoked;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    public static LRESULT TrayWindowSubclassProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam, nuint id, nuint data)
    {
        Invoked?.Invoke(hwnd, msg, wParam, lParam, id, data);
        return PInvoke.DefSubclassProc(hwnd, msg, wParam, lParam);
    }
}
