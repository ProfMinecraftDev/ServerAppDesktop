

namespace ServerAppDesktop.Helpers;

public static class ClipboardHelper
{
    public static void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        unsafe
        {

            if (!PInvoke.OpenClipboard(HWND.Null))
            {
                return;
            }

            try
            {
                _ = PInvoke.EmptyClipboard();


                uint charCount = (text.Length + 1).To<uint>();
                uint bytesCount = charCount * 2;


                HGLOBAL hGlobal = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, bytesCount);
                if (hGlobal == nint.Zero)
                {
                    return;
                }

                void* pTarget = PInvoke.GlobalLock(hGlobal);
                if (pTarget != null)
                {
                    fixed (char* pText = text)
                    {
                        System.Buffer.MemoryCopy(pText, pTarget, bytesCount, bytesCount);
                    }
                    _ = PInvoke.GlobalUnlock(hGlobal);
                }


                if (PInvoke.SetClipboardData(13, (HANDLE)(nint)hGlobal).IsNull)
                {
                    _ = PInvoke.GlobalFree(hGlobal);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error nativo: {ex.Message}");
            }
            finally
            {
                _ = PInvoke.CloseClipboard();
            }
        }
    }
}
