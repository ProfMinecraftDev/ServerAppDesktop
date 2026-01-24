using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;

namespace ServerAppDesktop.Helpers;

public static class ClipboardHelper
{
    public static void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        unsafe
        {
            // 1. Abrir el portapapeles (HWND nulo es seguro aquí)
            if (!PInvoke.OpenClipboard(HWND.Null))
                return;

            try
            {
                PInvoke.EmptyClipboard();

                // 2. Preparar la memoria global para el texto (Unicode)
                uint charCount = (uint)text.Length + 1;
                uint bytesCount = charCount * 2;

                // GMEM_MOVEABLE = 0x0002
                HGLOBAL hGlobal = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, bytesCount);
                if (hGlobal == IntPtr.Zero)
                    return;

                // 3. Bloquear memoria y copiar el string
                void* pTarget = PInvoke.GlobalLock(hGlobal);
                if (pTarget != null)
                {
                    fixed (char* pText = text)
                    {
                        System.Buffer.MemoryCopy(pText, pTarget, bytesCount, bytesCount);
                    }
                    PInvoke.GlobalUnlock(hGlobal);
                }

                // 4. Enviar al portapapeles (13 = CF_UNICODETEXT)
                if (PInvoke.SetClipboardData(13, (HANDLE)(IntPtr)hGlobal).IsNull)
                {
                    PInvoke.GlobalFree(hGlobal);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error nativo: {ex.Message}");
            }
            finally
            {
                PInvoke.CloseClipboard();
            }
        }
    }
}