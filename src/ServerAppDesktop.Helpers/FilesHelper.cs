using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ServerAppDesktop.Helpers;

public static partial class FilesHelper
{
    // Guids necesarios
    private static Guid CLSID_FileOpenDialog = new("D57C7288-D4AD-4768-BE02-9D969532D960");
    private static Guid IID_IFileOpenDialog = new("DC1C5A9C-2D8A-4D57-B5C4-CD0609AF9505");
    private static Guid IID_IShellItem = new("43826d1e-e718-42ee-bc55-a1e261c37bfe");

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(
        ref Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        ref Guid riid,
        out IntPtr ppv);

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int SHCreateItemFromParsingName(string pszPath, IntPtr pbc, ref Guid riid, out IntPtr ppv);

    public static string? OpenFileDialog(IntPtr hwnd, string title, string initialPath, string[] filters)
    {
        IFileOpenDialog? dialog = null;
        try
        {
            // Crear instancia mediante CoCreateInstance (Evita el error 80040154)
            int hr = CoCreateInstance(ref CLSID_FileOpenDialog, IntPtr.Zero, 1, ref IID_IFileOpenDialog, out IntPtr ptr);
            if (hr != 0)
                return null;
            dialog = (IFileOpenDialog)Marshal.GetObjectForIUnknown(ptr);

            dialog.SetTitle(title);

            if (filters.Length > 0)
            {
                var specs = new List<COMDLG_FILTERSPEC>();
                foreach (var f in filters)
                {
                    specs.Add(new COMDLG_FILTERSPEC { pszName = f, pszSpec = f });
                }
                var specsArray = specs.ToArray();
                dialog.SetFileTypes((uint)specsArray.Length, specsArray);
            }

            SetInitialFolder(dialog, initialPath);

            if (dialog.Show(hwnd) == 0)
            {
                dialog.GetResult(out IShellItem result);
                result.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out string path);
                return path;
            }
        }
        catch { }
        return null;
    }

    public static string? OpenFolderDialog(IntPtr hwnd, string title, string initialPath)
    {
        IFileOpenDialog? dialog = null;
        try
        {
            int hr = CoCreateInstance(ref CLSID_FileOpenDialog, IntPtr.Zero, 1, ref IID_IFileOpenDialog, out IntPtr ptr);
            if (hr != 0)
                return null;
            dialog = (IFileOpenDialog)Marshal.GetObjectForIUnknown(ptr);

            dialog.SetTitle(title);
            dialog.SetOptions(FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS | FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);

            SetInitialFolder(dialog, initialPath);

            if (dialog.Show(hwnd) == 0)
            {
                dialog.GetResult(out IShellItem result);
                result.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out string path);
                return path;
            }
        }
        catch { }
        return null;
    }

    private static void SetInitialFolder(IFileOpenDialog dialog, string path)
    {
        if (string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path))
            return;

        if (SHCreateItemFromParsingName(path, IntPtr.Zero, ref IID_IShellItem, out IntPtr shellItemPtr) == 0)
        {
            var shellItem = (IShellItem)Marshal.GetObjectForIUnknown(shellItemPtr);
            dialog.SetFolder(shellItem);
        }
    }

    #region Interfaces (Se mantienen igual, pero quita la clase vacía FileOpenDialog)
    [ComImport, Guid("DC1C5A9C-2D8A-4D57-B5C4-CD0609AF9505"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        [PreserveSig] int Show(IntPtr parent);
        void SetFileTypes(uint cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] COMDLG_FILTERSPEC[] rgFilterSpec);
        void SetFileTypeIndex(uint iFileType);
        void GetFileTypeIndex(out uint piFileType);
        void Advise(); void Unadvise();
        void SetOptions(FILEOPENDIALOGOPTIONS fos);
        void GetOptions(out FILEOPENDIALOGOPTIONS fos);
        void SetDefaultFolder(IShellItem psi);
        void SetFolder(IShellItem psi);
        void GetFolder(out IShellItem ppsi);
        void GetCurrentSelection(out IShellItem ppsi);
        void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetFileName([Out, MarshalAs(UnmanagedType.LPWStr)] out string pszName);
        void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        void GetResult(out IShellItem ppsi);
        void AddPlace(IShellItem psi, int fdap);
        void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
        void Close([MarshalAs(UnmanagedType.Error)] int hr);
        void SetClientGuid(ref Guid guid);
        void ClearClientData();
        void SetFilter([MarshalAs(UnmanagedType.IUnknown)] object pFilter);
    }

    [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        void BindToHandler(); void GetParent();
        void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
        void GetAttributes(); void Compare();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct COMDLG_FILTERSPEC { public string pszName; public string pszSpec; }

    [Flags]
    private enum FILEOPENDIALOGOPTIONS : uint
    {
        FOS_PICKFOLDERS = 0x00000020,
        FOS_FORCEFILESYSTEM = 0x00000040
    }
    private enum SIGDN : uint { SIGDN_FILESYSPATH = 0x80058000 }
    #endregion
}