namespace ServerAppDesktop.Helpers;

public static class ClipboardHelper
{
    private static bool TryOpenClipboard() => PInvoke.OpenClipboard(HWND.Null);

    public static bool HasFileContent()
    {
        if (!PInvoke.OpenClipboard(HWND.Null))
            return false;

        try
        {
            return PInvoke.IsClipboardFormatAvailable(15);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(ResourceHelper.GetString("ErrorToQueryFormat"), ex.Message));
            return false;
        }
        finally
        {
            _ = PInvoke.CloseClipboard();
        }
    }

    public static void SetText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        unsafe
        {

            if (!TryOpenClipboard())
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
                System.Diagnostics.Debug.WriteLine(string.Format(ResourceHelper.GetString("NativeError"), ex.Message));
            }
            finally
            {
                _ = PInvoke.CloseClipboard();
            }
        }
    }

    public static void SetFile(string path) => SetFileSystemItem([path]);

    public static void SetFolder(string path) => SetFileSystemItem([path]);

    private static unsafe void SetFileSystemItem(string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return;

        if (!TryOpenClipboard())
            return;

        try
        {
            _ = PInvoke.EmptyClipboard();

            int dropFilesSize = sizeof(DROPFILES);
            int pathsSize = paths.Sum(p => (p.Length + 1) * 2) + 2;
            uint totalSize = (uint)(dropFilesSize + pathsSize);

            HGLOBAL hGlobal = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE | GLOBAL_ALLOC_FLAGS.GMEM_ZEROINIT, totalSize);
            if (hGlobal == nint.Zero)
                return;

            void* pTarget = PInvoke.GlobalLock(hGlobal);
            if (pTarget != null)
            {
                var pDropFiles = (DROPFILES*)pTarget;
                pDropFiles->pFiles = (uint)dropFilesSize;
                pDropFiles->fWide = true;

                char* pPathStart = (char*)((byte*)pTarget + dropFilesSize);
                int currentOffset = 0;

                foreach (string path in paths)
                {
                    fixed (char* pSource = path)
                    {
                        System.Buffer.MemoryCopy(pSource, pPathStart + currentOffset, path.Length * 2, path.Length * 2);
                    }
                    currentOffset += path.Length + 1;
                }

                _ = PInvoke.GlobalUnlock(hGlobal);
            }

            if (PInvoke.SetClipboardData(15, (HANDLE)(nint)hGlobal).IsNull)
            {
                _ = PInvoke.GlobalFree(hGlobal);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(string.Format(ResourceHelper.GetString("ClipboardError"), ex.Message));
        }
        finally
        {
            _ = PInvoke.CloseClipboard();
        }
    }

    public static unsafe string[] GetPaths()
    {
        if (!PInvoke.OpenClipboard(HWND.Null))
            return [];

        try
        {
            HANDLE hData = PInvoke.GetClipboardData(15);
            if (hData.IsNull)
                return [];

            void* pDropFiles = PInvoke.GlobalLock((HGLOBAL)(nint)hData);
            if (pDropFiles == null)
                return [];

            try
            {
                var drop = (DROPFILES*)pDropFiles;
                var paths = new List<string>();

                char* pStart = (char*)((byte*)pDropFiles + drop->pFiles);

                while (*pStart != '\0')
                {
                    string path = new(pStart);
                    paths.Add(path);
                    pStart += path.Length + 1;
                }
                return [.. paths];
            }
            finally
            {
                _ = PInvoke.GlobalUnlock((HGLOBAL)(nint)hData);
            }
        }
        finally
        {
            _ = PInvoke.CloseClipboard();
        }
    }

    public static unsafe void SetMoveEffect()
    {
        string formatName = "Preferred DropEffect";
        uint formatId = PInvoke.RegisterClipboardFormat(formatName);

        if (!PInvoke.OpenClipboard(HWND.Null))
            return;

        try
        {
            HGLOBAL hGlobal = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(uint));
            if (hGlobal == nint.Zero)
                return;

            uint* pData = (uint*)PInvoke.GlobalLock(hGlobal);
            if (pData != null)
            {
                *pData = 2;
                _ = PInvoke.GlobalUnlock(hGlobal);
            }

            if (PInvoke.SetClipboardData(formatId, (HANDLE)(nint)hGlobal).IsNull)
            {
                _ = PInvoke.GlobalFree(hGlobal);
            }
        }
        finally
        {
            _ = PInvoke.CloseClipboard();
        }
    }

    public static unsafe uint GetDropEffect()
    {
        if (!PInvoke.OpenClipboard(HWND.Null))
            return 0;
        try
        {
            uint formatId = PInvoke.RegisterClipboardFormat("Preferred DropEffect");
            HANDLE hData = PInvoke.GetClipboardData(formatId);
            if (hData.IsNull)
                return 1;

            uint* pData = (uint*)PInvoke.GlobalLock((HGLOBAL)(nint)hData);
            if (pData == null)
                return 1;

            uint effect = *pData;
            _ = PInvoke.GlobalUnlock((HGLOBAL)(nint)hData);
            return effect;
        }
        finally { _ = PInvoke.CloseClipboard(); }
    }
}
